using Invoices.Application.Import.Source;

namespace Invoices.UnitTests.Import;

internal static class ImportedInvoiceBuilder
{
    public static readonly DateOnly InvoiceDate = new(2025, 1, 1);

    public static ImportedInvoice Build(
        long total = 1_000,
        IReadOnlyList<ImportedLine>? lines = null,
        IReadOnlyList<ImportedCreditNote>? creditNotes = null,
        ImportedPayment? payment = null,
        string? declaredStatus = "issued",
        string? declaredPaymentStatus = "Pending",
        string run = "13075795-2",
        int daysToDue = 30,
        DateOnly? paymentDueDate = null) =>
        new(
            InvoiceNumber: 1,
            InvoiceDate: InvoiceDate,
            DeclaredInvoiceStatus: declaredStatus,
            TotalAmount: total,
            DaysToDue: daysToDue,
            PaymentDueDate: paymentDueDate ?? InvoiceDate.AddDays(daysToDue),
            DeclaredPaymentStatus: declaredPaymentStatus,
            Lines: lines ?? [new ImportedLine("Producto", 100, 10, 1_000)],
            Payment: payment,
            CreditNotes: creditNotes ?? [],
            Customer: new ImportedCustomer(run, "Cliente", null));
}
