using Invoices.Domain.Invoices;

namespace Invoices.Application.Abstractions;

/// <summary>Repositorio del agregado <see cref="Invoice"/> (lado de escritura).</summary>
public interface IInvoiceRepository
{
    /// <summary>Factura activa (consistente) con sus notas de crédito, lista para modificarse.</summary>
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
