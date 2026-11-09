using Invoices.Application.Abstractions;
using Invoices.Domain.Common;
using Invoices.Domain.Invoices;

namespace Invoices.Application.Reports;

public interface IReportService
{
    Task<Result<OverdueInvoicesReportDto>> GetOverdueInvoicesAsync(
        int minDaysOverdue,
        DateOnly? asOf,
        CancellationToken cancellationToken);

    Task<PaymentStatusSummaryDto> GetPaymentStatusSummaryAsync(DateOnly? asOf, CancellationToken cancellationToken);

    Task<IReadOnlyList<InconsistentInvoiceDto>> GetInconsistentInvoicesAsync(CancellationToken cancellationToken);
}

public sealed class ReportService(IInvoiceReadRepository repository, IBusinessCalendar calendar) : IReportService
{
    public const int DefaultMinDaysOverdue = 30;

    private static readonly PaymentStatus[] SummaryOrder = [PaymentStatus.Paid, PaymentStatus.Pending, PaymentStatus.Overdue];

    public async Task<Result<OverdueInvoicesReportDto>> GetOverdueInvoicesAsync(
        int minDaysOverdue,
        DateOnly? asOf,
        CancellationToken cancellationToken)
    {
        if (minDaysOverdue < 0)
        {
            return Error.Validation("report.invalid_days", "La cantidad de días vencidos no puede ser negativa.");
        }

        var today = asOf ?? calendar.Today;
        var items = await repository.GetOverdueWithoutPaymentOrCreditNotesAsync(minDaysOverdue, today, cancellationToken);
        return new OverdueInvoicesReportDto(today, minDaysOverdue, items.Count, items.Sum(item => item.TotalAmount), items);
    }

    public async Task<PaymentStatusSummaryDto> GetPaymentStatusSummaryAsync(
        DateOnly? asOf,
        CancellationToken cancellationToken)
    {
        var today = asOf ?? calendar.Today;
        var counts = await repository.CountByPaymentStatusAsync(today, cancellationToken);
        var total = counts.Values.Sum();

        var items = SummaryOrder
            .Select(status =>
            {
                var count = counts.GetValueOrDefault(status);
                var percentage = total == 0 ? 0m : Math.Round(count * 100m / total, 1, MidpointRounding.AwayFromZero);
                return new PaymentStatusCountDto(status, count, percentage);
            })
            .ToList();

        return new PaymentStatusSummaryDto(today, total, items);
    }

    public Task<IReadOnlyList<InconsistentInvoiceDto>> GetInconsistentInvoicesAsync(CancellationToken cancellationToken) =>
        repository.GetInconsistentAsync(cancellationToken);
}
