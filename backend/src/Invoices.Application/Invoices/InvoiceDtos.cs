using Invoices.Domain.Invoices;

namespace Invoices.Application.Invoices;

/// <summary>Criterios de búsqueda de facturas activas.</summary>
public sealed record InvoiceSearchCriteria(
    int? InvoiceNumber,
    InvoiceStatus? InvoiceStatus,
    PaymentStatus? PaymentStatus,
    int Page = 1,
    int PageSize = 20);

/// <summary>Fila del listado de facturas activas.</summary>
public sealed class InvoiceListItemDto
{
    public int InvoiceNumber { get; init; }

    public DateOnly InvoiceDate { get; init; }

    public DateOnly PaymentDueDate { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string CustomerRun { get; init; } = string.Empty;

    public long TotalAmount { get; init; }

    public long CreditNotesTotal { get; init; }

    public long PendingBalance { get; init; }

    public InvoiceStatus InvoiceStatus { get; init; }

    public PaymentStatus PaymentStatus { get; init; }
}

public sealed record CustomerDto(string Run, string Name, string? Email);

public sealed record PaymentDto(string? Method, DateOnly PaymentDate);

public sealed record InvoiceLineDto(string ProductName, long UnitPrice, int Quantity, long Subtotal);

public sealed record CreditNoteDto(int Number, DateOnly Date, long Amount);

public sealed record ImportIssueDto(string Code, ImportIssueSeverity Severity, string Message);

/// <summary>Detalle completo de una factura activa.</summary>
public sealed record InvoiceDetailDto(
    int InvoiceNumber,
    DateOnly InvoiceDate,
    int DaysToDue,
    DateOnly PaymentDueDate,
    long TotalAmount,
    long CreditNotesTotal,
    long PendingBalance,
    InvoiceStatus InvoiceStatus,
    PaymentStatus PaymentStatus,
    int? DaysOverdue,
    DateOnly AsOf,
    CustomerDto Customer,
    PaymentDto? Payment,
    IReadOnlyList<InvoiceLineDto> Lines,
    IReadOnlyList<CreditNoteDto> CreditNotes,
    IReadOnlyList<ImportIssueDto> ImportIssues);
