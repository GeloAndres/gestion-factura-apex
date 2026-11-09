using Invoices.Domain.Invoices;

namespace Invoices.Infrastructure.Persistence.Views;

public sealed class InvoiceSummaryView
{
    public int InvoiceId { get; private set; }

    public int InvoiceNumber { get; private set; }

    public DateOnly InvoiceDate { get; private set; }

    public int DaysToDue { get; private set; }

    public DateOnly PaymentDueDate { get; private set; }

    public long TotalAmount { get; private set; }

    public string CustomerRun { get; private set; } = string.Empty;

    public string CustomerName { get; private set; } = string.Empty;

    public int CreditNotesCount { get; private set; }

    public long CreditNotesTotal { get; private set; }

    public long PendingBalance { get; private set; }

    public InvoiceStatus InvoiceStatus { get; private set; }

    public DateOnly? PaymentDate { get; private set; }

    public string? PaymentMethod { get; private set; }
}

public sealed class InconsistentInvoiceView
{
    public int InvoiceId { get; private set; }

    public int InvoiceNumber { get; private set; }

    public DateOnly InvoiceDate { get; private set; }

    public string CustomerRun { get; private set; } = string.Empty;

    public string CustomerName { get; private set; } = string.Empty;

    public long DeclaredTotal { get; private set; }

    public long LinesTotal { get; private set; }

    public long Difference { get; private set; }
}
