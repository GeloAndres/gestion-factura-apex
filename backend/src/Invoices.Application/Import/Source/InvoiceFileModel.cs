namespace Invoices.Application.Import.Source;

// Estructura del archivo JSON de origen (bd_exam.json). Todos los campos son opcionales a propósito:
// así un registro incompleto se descarta con un motivo claro sin invalidar el archivo completo.

public sealed class InvoiceFileModel
{
    public List<InvoiceRecord?>? Invoices { get; set; }
}

public sealed class InvoiceRecord
{
    public int? InvoiceNumber { get; set; }

    public DateOnly? InvoiceDate { get; set; }

    public string? InvoiceStatus { get; set; }

    public long? TotalAmount { get; set; }

    public int? DaysToDue { get; set; }

    public DateOnly? PaymentDueDate { get; set; }

    public string? PaymentStatus { get; set; }

    public List<InvoiceDetailRecord?>? InvoiceDetail { get; set; }

    public InvoicePaymentRecord? InvoicePayment { get; set; }

    public List<CreditNoteRecord?>? InvoiceCreditNote { get; set; }

    public CustomerRecord? Customer { get; set; }
}

public sealed class InvoiceDetailRecord
{
    public string? ProductName { get; set; }

    public long? UnitPrice { get; set; }

    public int? Quantity { get; set; }

    public long? Subtotal { get; set; }
}

public sealed class InvoicePaymentRecord
{
    public string? PaymentMethod { get; set; }

    public DateOnly? PaymentDate { get; set; }
}

public sealed class CreditNoteRecord
{
    public int? CreditNoteNumber { get; set; }

    public DateOnly? CreditNoteDate { get; set; }

    public long? CreditNoteAmount { get; set; }
}

public sealed class CustomerRecord
{
    public string? CustomerRun { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }
}
