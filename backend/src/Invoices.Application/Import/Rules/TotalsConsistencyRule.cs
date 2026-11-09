using Invoices.Application.Import.Source;
using Invoices.Domain.Common;

namespace Invoices.Application.Import.Rules;

public sealed class TotalsConsistencyRule : IInvoiceImportRule
{
    public IEnumerable<ImportFinding> Evaluate(ImportedInvoice invoice)
    {
        var linesTotal = invoice.Lines.Sum(line => line.Subtotal);
        if (linesTotal != invoice.TotalAmount)
        {
            yield return ImportFinding.Inconsistency(
                ImportIssueCodes.TotalMismatch,
                $"La suma de los subtotales ({Money.Format(linesTotal)}) no coincide con el total declarado " +
                $"({Money.Format(invoice.TotalAmount)}). Diferencia: {Money.Format(invoice.TotalAmount - linesTotal)}.");
        }
    }
}
