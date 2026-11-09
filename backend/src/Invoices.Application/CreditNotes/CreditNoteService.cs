using Invoices.Application.Abstractions;
using Invoices.Domain.Common;
using Invoices.Domain.Invoices;
using Microsoft.Extensions.Logging;

namespace Invoices.Application.CreditNotes;

public sealed record CreditNoteCreatedDto(
    int InvoiceNumber,
    int Number,
    DateOnly Date,
    long Amount,
    long PendingBalance,
    InvoiceStatus InvoiceStatus);

public interface ICreditNoteService
{
    Task<Result<CreditNoteCreatedDto>> AddAsync(int invoiceNumber, long amount, CancellationToken cancellationToken);
}

public sealed class CreditNoteService(
    IInvoiceRepository invoices,
    IUnitOfWork unitOfWork,
    IBusinessCalendar calendar,
    ILogger<CreditNoteService> logger) : ICreditNoteService
{
    public async Task<Result<CreditNoteCreatedDto>> AddAsync(
        int invoiceNumber,
        long amount,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            return InvoiceErrors.InvalidCreditNoteAmount;
        }

        return await unitOfWork.ExecuteInTransactionAsync<CreditNoteCreatedDto>(
            async ct =>
            {
                var invoice = await invoices.GetActiveForUpdateAsync(invoiceNumber, ct);
                if (invoice is null)
                {
                    return InvoiceErrors.NotFound(invoiceNumber);
                }

                var number = await invoices.GetNextCreditNoteNumberAsync(ct);
                var added = invoice.AddCreditNote(number, amount, calendar.Today);
                if (added.IsFailure)
                {
                    return added.Error;
                }

                await unitOfWork.SaveChangesAsync(ct);

                logger.LogInformation(
                    "Nota de crédito {CreditNoteNumber} por {Amount} agregada a la factura {InvoiceNumber}",
                    added.Value.Number,
                    added.Value.Amount,
                    invoiceNumber);

                return new CreditNoteCreatedDto(
                    invoice.InvoiceNumber,
                    added.Value.Number,
                    added.Value.Date,
                    added.Value.Amount,
                    invoice.PendingBalance,
                    invoice.Status);
            },
            cancellationToken);
    }
}
