using Invoices.Application.Import.Source;
using Invoices.Domain.Common;

namespace Invoices.Application.Import.Rules;

/// <summary>Advierte cuando las notas de crédito del archivo suman más que el total de la factura.</summary>
public sealed class CreditNotesWithinTotalRule : IInvoiceImportRule
{
    public IEnumerable<ImportFinding> Evaluate(ImportedInvoice invoice)
    {
        var creditNotesTotal = invoice.CreditNotes.Sum(note => note.Amount);
        if (creditNotesTotal > invoice.TotalAmount)
        {
            yield return ImportFinding.Warning(
                ImportIssueCodes.CreditNotesExceedTotal,
                $"Las notas de crédito suman {Money.Format(creditNotesTotal)} y superan el total de la factura " +
                $"({Money.Format(invoice.TotalAmount)}).");
        }
    }
}
