namespace Invoices.Domain.Invoices;

public static class InvoiceStatusRules
{
    public static InvoiceStatus Resolve(long totalAmount, long creditNotesTotal) =>
        creditNotesTotal <= 0 ? InvoiceStatus.Issued
        : creditNotesTotal >= totalAmount ? InvoiceStatus.Cancelled
        : InvoiceStatus.Partial;
}
