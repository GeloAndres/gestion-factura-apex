using Invoices.Application.Abstractions;
using Invoices.Application.Common;
using Invoices.Application.Invoices;
using Invoices.Application.Reports;
using Invoices.Domain.Invoices;
using Invoices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Invoices.Infrastructure.Repositories;

internal sealed class InvoiceReadRepository(InvoicesDbContext dbContext) : IInvoiceReadRepository
{
    public async Task<PagedResult<InvoiceListItemDto>> SearchAsync(
        InvoiceSearchCriteria criteria,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var query = ProjectSummaries(today);

        if (criteria.InvoiceNumber is { } invoiceNumber)
        {
            query = query.Where(row => row.InvoiceNumber == invoiceNumber);
        }

        if (criteria.InvoiceStatus is { } invoiceStatus)
        {
            query = query.Where(row => row.InvoiceStatus == invoiceStatus);
        }

        if (criteria.PaymentStatus is { } paymentStatus)
        {
            query = query.Where(row => row.PaymentStatus == paymentStatus);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(row => row.InvoiceNumber)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<InvoiceListItemDto>(items, criteria.Page, criteria.PageSize, totalItems);
    }

    public async Task<InvoiceDetailDto?> GetDetailAsync(
        int invoiceNumber,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var summary = await ProjectSummaries(today)
            .FirstOrDefaultAsync(row => row.InvoiceNumber == invoiceNumber, cancellationToken);
        if (summary is null)
        {
            return null;
        }

        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .AsSplitQuery()
            .Include(entity => entity.Customer)
            .Include(entity => entity.Lines)
            .Include(entity => entity.CreditNotes)
            .Include(entity => entity.ImportIssues)
            .SingleAsync(entity => entity.InvoiceNumber == invoiceNumber, cancellationToken);

        return new InvoiceDetailDto(
            summary.InvoiceNumber,
            summary.InvoiceDate,
            invoice.DaysToDue,
            summary.PaymentDueDate,
            summary.TotalAmount,
            summary.CreditNotesTotal,
            summary.PendingBalance,
            summary.InvoiceStatus,
            summary.PaymentStatus,
            summary.PaymentStatus == PaymentStatus.Overdue
                ? PaymentStatusRules.DaysOverdue(summary.PaymentDueDate, today)
                : null,
            today,
            new CustomerDto(invoice.Customer.Run, invoice.Customer.Name, invoice.Customer.Email),
            invoice.Payment is null ? null : new PaymentDto(invoice.Payment.Method, invoice.Payment.PaymentDate),
            invoice.Lines
                .OrderBy(line => line.Id)
                .Select(line => new InvoiceLineDto(line.ProductName, line.UnitPrice, line.Quantity, line.Subtotal))
                .ToList(),
            invoice.CreditNotes
                .OrderBy(note => note.Date)
                .ThenBy(note => note.Number)
                .Select(note => new CreditNoteDto(note.Number, note.Date, note.Amount))
                .ToList(),
            invoice.ImportIssues
                .OrderBy(issue => issue.Id)
                .Select(issue => new ImportIssueDto(issue.Code, issue.Severity, issue.Message))
                .ToList());
    }

    public async Task<IReadOnlyList<OverdueInvoiceDto>> GetOverdueWithoutPaymentOrCreditNotesAsync(
        int minDaysOverdue,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var cutoff = today.AddDays(-minDaysOverdue);

        var rows = await dbContext.InvoiceSummaries
            .AsNoTracking()
            .Where(view => view.PaymentDate == null && view.CreditNotesCount == 0 && view.PaymentDueDate < cutoff)
            .OrderBy(view => view.PaymentDueDate)
            .ThenBy(view => view.InvoiceNumber)
            .Select(view => new
            {
                view.InvoiceNumber,
                view.InvoiceDate,
                view.PaymentDueDate,
                view.CustomerName,
                view.CustomerRun,
                view.TotalAmount,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new OverdueInvoiceDto(
                row.InvoiceNumber,
                row.InvoiceDate,
                row.PaymentDueDate,
                PaymentStatusRules.DaysOverdue(row.PaymentDueDate, today),
                row.CustomerName,
                row.CustomerRun,
                row.TotalAmount))
            .ToList();
    }

    public async Task<IReadOnlyDictionary<PaymentStatus, int>> CountByPaymentStatusAsync(
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var groups = await ProjectSummaries(today)
            .GroupBy(row => row.PaymentStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        return groups.ToDictionary(group => group.Status, group => group.Count);
    }

    public async Task<IReadOnlyList<InconsistentInvoiceDto>> GetInconsistentAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.InconsistentInvoices
            .AsNoTracking()
            .OrderBy(view => view.InvoiceNumber)
            .ToListAsync(cancellationToken);

        var invoiceIds = rows.Select(row => row.InvoiceId).ToList();
        var linesByInvoice = (await dbContext.InvoiceLines
                .AsNoTracking()
                .Where(line => invoiceIds.Contains(line.InvoiceId))
                .OrderBy(line => line.Id)
                .ToListAsync(cancellationToken))
            .ToLookup(line => line.InvoiceId);

        return rows
            .Select(row => new InconsistentInvoiceDto(
                row.InvoiceNumber,
                row.InvoiceDate,
                row.CustomerName,
                row.CustomerRun,
                row.DeclaredTotal,
                row.LinesTotal,
                row.Difference,
                linesByInvoice[row.InvoiceId]
                    .Select(line => new InconsistentLineDto(
                        line.ProductName,
                        line.UnitPrice,
                        line.Quantity,
                        line.Subtotal,
                        line.ExpectedSubtotal))
                    .ToList()))
            .ToList();
    }

    private IQueryable<InvoiceListItemDto> ProjectSummaries(DateOnly today) =>
        dbContext.InvoiceSummaries
            .AsNoTracking()
            .Select(view => new InvoiceListItemDto
            {
                InvoiceNumber = view.InvoiceNumber,
                InvoiceDate = view.InvoiceDate,
                PaymentDueDate = view.PaymentDueDate,
                CustomerName = view.CustomerName,
                CustomerRun = view.CustomerRun,
                TotalAmount = view.TotalAmount,
                CreditNotesTotal = view.CreditNotesTotal,
                PendingBalance = view.PendingBalance,
                InvoiceStatus = view.InvoiceStatus,
                PaymentStatus = view.PaymentDate != null
                    ? PaymentStatus.Paid
                    : view.PaymentDueDate < today
                        ? PaymentStatus.Overdue
                        : PaymentStatus.Pending,
            });
}
