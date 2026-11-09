using Invoices.Application.Abstractions;
using Invoices.Domain.Invoices;
using Invoices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Invoices.Infrastructure.Repositories;

internal sealed class InvoiceRepository(InvoicesDbContext dbContext) : IInvoiceRepository
{
    public Task<Invoice?> GetActiveForUpdateAsync(int invoiceNumber, CancellationToken cancellationToken) =>
        dbContext.Invoices
            .Include(invoice => invoice.CreditNotes)
            .FirstOrDefaultAsync(
                invoice => invoice.InvoiceNumber == invoiceNumber && invoice.IsConsistent,
                cancellationToken);

    public async Task<int> GetNextCreditNoteNumberAsync(CancellationToken cancellationToken) =>
        (await dbContext.CreditNotes.MaxAsync(note => (int?)note.Number, cancellationToken) ?? 0) + 1;

    public async Task<IReadOnlySet<int>> GetExistingInvoiceNumbersAsync(
        IReadOnlyCollection<int> invoiceNumbers,
        CancellationToken cancellationToken)
    {
        if (invoiceNumbers.Count == 0)
        {
            return new HashSet<int>();
        }

        var existing = await dbContext.Invoices
            .Where(invoice => invoiceNumbers.Contains(invoice.InvoiceNumber))
            .Select(invoice => invoice.InvoiceNumber)
            .ToListAsync(cancellationToken);
        return existing.ToHashSet();
    }

    public async Task<IReadOnlySet<int>> GetExistingCreditNoteNumbersAsync(
        IReadOnlyCollection<int> creditNoteNumbers,
        CancellationToken cancellationToken)
    {
        if (creditNoteNumbers.Count == 0)
        {
            return new HashSet<int>();
        }

        var existing = await dbContext.CreditNotes
            .Where(note => creditNoteNumbers.Contains(note.Number))
            .Select(note => note.Number)
            .ToListAsync(cancellationToken);
        return existing.ToHashSet();
    }

    public void Add(Invoice invoice) => dbContext.Invoices.Add(invoice);
}
