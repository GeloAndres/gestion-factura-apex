using Invoices.Application.Common;
using Invoices.Application.Invoices;
using Invoices.Application.Reports;
using Invoices.Domain.Invoices;

namespace Invoices.Application.Abstractions;

/// <summary>
/// Consultas de solo lectura sobre las vistas SQL (vw_invoice_summary y vw_inconsistent_invoices).
/// <paramref name="today"/> es la fecha con la que se calcula el estado de pago.
/// </summary>
public interface IInvoiceReadRepository
{
    Task<PagedResult<InvoiceListItemDto>> SearchAsync(
        InvoiceSearchCriteria criteria,
        DateOnly today,
        CancellationToken cancellationToken);

    Task<InvoiceDetailDto?> GetDetailAsync(int invoiceNumber, DateOnly today, CancellationToken cancellationToken);

    /// <summary>Facturas activas sin pago ni notas de crédito, vencidas hace más de <paramref name="minDaysOverdue"/> días.</summary>
    Task<IReadOnlyList<OverdueInvoiceDto>> GetOverdueWithoutPaymentOrCreditNotesAsync(
        int minDaysOverdue,
        DateOnly today,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<PaymentStatus, int>> CountByPaymentStatusAsync(
        DateOnly today,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<InconsistentInvoiceDto>> GetInconsistentAsync(CancellationToken cancellationToken);
}
