using Invoices.Application.Common;
using Invoices.Application.Invoices;
using Invoices.Application.Reports;
using Invoices.Domain.Invoices;

namespace Invoices.Application.Abstractions;

public interface IInvoiceReadRepository
{
    Task<PagedResult<InvoiceListItemDto>> SearchAsync(
        InvoiceSearchCriteria criteria,
        DateOnly today,
        CancellationToken cancellationToken);

    Task<InvoiceDetailDto?> GetDetailAsync(int invoiceNumber, DateOnly today, CancellationToken cancellationToken);

    Task<IReadOnlyList<OverdueInvoiceDto>> GetOverdueWithoutPaymentOrCreditNotesAsync(
        int minDaysOverdue,
        DateOnly today,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<PaymentStatus, int>> CountByPaymentStatusAsync(
        DateOnly today,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<InconsistentInvoiceDto>> GetInconsistentAsync(CancellationToken cancellationToken);
}
