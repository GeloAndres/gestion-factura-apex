using Invoices.Domain.Invoices;

namespace Invoices.Application.Abstractions;

public interface IInvoiceRepository
{
    Task<Invoice?> GetActiveForUpdateAsync(int invoiceNumber, CancellationToken cancellationToken);

    Task<int> GetNextCreditNoteNumberAsync(CancellationToken cancellationToken);

    Task<IReadOnlySet<int>> GetExistingInvoiceNumbersAsync(
        IReadOnlyCollection<int> invoiceNumbers,
        CancellationToken cancellationToken);

    Task<IReadOnlySet<int>> GetExistingCreditNoteNumbersAsync(
        IReadOnlyCollection<int> creditNoteNumbers,
        CancellationToken cancellationToken);

    void Add(Invoice invoice);
}
