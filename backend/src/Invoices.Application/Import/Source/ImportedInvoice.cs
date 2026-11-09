namespace Invoices.Application.Import.Source;

// Registro ya validado estructuralmente: las reglas de importación trabajan sobre datos completos.

public sealed record ImportedInvoice(
    int InvoiceNumber,
    DateOnly InvoiceDate,
    string? DeclaredInvoiceStatus,
    long TotalAmount,
    int DaysToDue,
    DateOnly PaymentDueDate,
    string? DeclaredPaymentStatus,
    IReadOnlyList<ImportedLine> Lines,
    ImportedPayment? Payment,
    IReadOnlyList<ImportedCreditNote> CreditNotes,
    ImportedCustomer Customer);

public sealed record ImportedLine(string ProductName, long UnitPrice, int Quantity, long Subtotal);

public sealed record ImportedPayment(string? Method, DateOnly PaymentDate);

public sealed record ImportedCreditNote(int Number, DateOnly Date, long Amount);

public sealed record ImportedCustomer(string Run, string Name, string? Email);
