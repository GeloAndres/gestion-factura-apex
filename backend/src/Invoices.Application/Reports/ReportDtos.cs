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

/// <summary>Facturas consistentes vencidas hace más de N días, sin pago ni notas de crédito.</summary>
public sealed record OverdueInvoicesReportDto(
    DateOnly AsOf,
    int MinDaysOverdue,
    int Count,
    long TotalAmount,
    IReadOnlyList<OverdueInvoiceDto> Items);

public sealed record PaymentStatusCountDto(PaymentStatus Status, int Count, decimal Percentage);

/// <summary>Total y porcentaje de facturas activas por estado de pago.</summary>
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
    /// <summary>Indica si el subtotal declarado coincide con precio × cantidad.</summary>
    public bool SubtotalMatches => Subtotal == ExpectedSubtotal;
}

/// <summary>Factura cuyo total declarado no coincide con la suma de los subtotales de sus productos.</summary>
public sealed record InconsistentInvoiceDto(
    int InvoiceNumber,
    DateOnly InvoiceDate,
    string CustomerName,
    string CustomerRun,
    long DeclaredTotal,
    long LinesTotal,
    long Difference,
    IReadOnlyList<InconsistentLineDto> Lines);
