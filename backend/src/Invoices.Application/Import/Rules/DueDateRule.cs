using Invoices.Application.Import.Source;

namespace Invoices.Application.Import.Rules;

/// <summary>
/// Advierte cuando invoice_date + days_to_due no da payment_due_date. Se usa payment_due_date,
/// que es el campo que el enunciado define para el vencimiento.
/// </summary>
public sealed class DueDateRule : IInvoiceImportRule
{
    public IEnumerable<ImportFinding> Evaluate(ImportedInvoice invoice)
    {
        var expected = invoice.InvoiceDate.AddDays(invoice.DaysToDue);
        if (expected != invoice.PaymentDueDate)
        {
            yield return ImportFinding.Warning(
                ImportIssueCodes.DueDateMismatch,
                $"invoice_date ({invoice.InvoiceDate:yyyy-MM-dd}) + days_to_due ({invoice.DaysToDue}) = {expected:yyyy-MM-dd}, " +
                $"pero payment_due_date es {invoice.PaymentDueDate:yyyy-MM-dd}. Se usa payment_due_date.");
        }
    }
}
