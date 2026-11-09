using Invoices.Domain.Invoices;

namespace Invoices.Application.Reports;

public sealed record OverdueInvoiceDto(
    int InvoiceNumber,
    DateOnly InvoiceDate,
    DateOnly PaymentDueDate,
    int DaysOverdue,
    string CustomerName,
    string CustomerRun,
    long TotalAmount);

public sealed record OverdueInvoicesReportDto(
    DateOnly AsOf,
    int MinDaysOverdue,
    int Count,
    long TotalAmount,
    IReadOnlyList<OverdueInvoiceDto> Items);

public sealed record PaymentStatusCountDto(PaymentStatus Status, int Count, decimal Percentage);

public sealed record PaymentStatusSummaryDto(
    DateOnly AsOf,
    int TotalInvoices,
    IReadOnlyList<PaymentStatusCountDto> Items);

public sealed record InconsistentLineDto(
    string ProductName,
    long UnitPrice,
    int Quantity,
    long Subtotal,
    long ExpectedSubtotal)
{
    public bool SubtotalMatches => Subtotal == ExpectedSubtotal;
}

public sealed record InconsistentInvoiceDto(
    int InvoiceNumber,
    DateOnly InvoiceDate,
    string CustomerName,
    string CustomerRun,
    long DeclaredTotal,
    long LinesTotal,
    long Difference,
    IReadOnlyList<InconsistentLineDto> Lines);
