using Invoices.Application.Import.Source;
using Invoices.Domain.Common;

namespace Invoices.Application.Import.Rules;

public sealed class LineSubtotalRule : IInvoiceImportRule
{
    public IEnumerable<ImportFinding> Evaluate(ImportedInvoice invoice)
    {
        foreach (var line in invoice.Lines)
        {
            var expected = line.UnitPrice * line.Quantity;
            if (expected != line.Subtotal)
            {
                yield return ImportFinding.Warning(
                    ImportIssueCodes.LineSubtotalMismatch,
                    $"'{line.ProductName}': {Money.Format(line.UnitPrice)} × {line.Quantity} = {Money.Format(expected)}, " +
                    $"pero el subtotal declarado es {Money.Format(line.Subtotal)}.");
            }
        }
    }
}
