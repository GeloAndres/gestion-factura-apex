using Invoices.Domain.Common;

namespace Invoices.Domain.Invoices;

public static class InvoiceErrors
{
    public static readonly Error InvalidCreditNoteAmount = Error.Validation(
        "credit_note.invalid_amount",
        "El monto de la nota de crédito debe ser un entero mayor a 0.");

    public static Error NotFound(int invoiceNumber) => Error.NotFound(
        "invoice.not_found",
        $"No existe una factura activa con el número {invoiceNumber}. Las facturas inconsistentes solo aparecen en su reporte.");

    public static Error Inconsistent(int invoiceNumber) => Error.BusinessRule(
        "invoice.inconsistent",
        $"La factura {invoiceNumber} es inconsistente y está excluida del sistema activo.");

    public static Error NoPendingBalance(int invoiceNumber) => Error.BusinessRule(
        "credit_note.no_pending_balance",
        $"La factura {invoiceNumber} no tiene saldo pendiente: sus notas de crédito ya suman el total.");

    public static Error CreditNoteExceedsPendingBalance(long amount, long pendingBalance) => Error.BusinessRule(
        "credit_note.exceeds_pending_balance",
        $"El monto de la nota de crédito ({Money.Format(amount)}) supera el saldo pendiente de la factura ({Money.Format(pendingBalance)}).");
}
