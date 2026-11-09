using Invoices.Application.Import.Source;
using Invoices.Domain.Invoices;

namespace Invoices.Application.Import.Rules;

/// <summary>
/// El estado de la factura se calcula siempre desde sus notas de crédito. Esta regla solo advierte
/// cuando el estado que trae el archivo no coincide con el calculado (ej.: "canceled" sin notas de crédito).
/// </summary>
public sealed class DeclaredInvoiceStatusRule : IInvoiceImportRule
{
    public IEnumerable<ImportFinding> Evaluate(ImportedInvoice invoice)
    {
        if (string.IsNullOrWhiteSpace(invoice.DeclaredInvoiceStatus))
        {
            yield break;
        }

        var declared = invoice.DeclaredInvoiceStatus.Trim();
        var computed = InvoiceStatusRules.Resolve(invoice.TotalAmount, invoice.CreditNotes.Sum(note => note.Amount));

        switch (Parse(declared))
        {
            case null:
                yield return ImportFinding.Warning(
                    ImportIssueCodes.UnknownDeclaredStatus,
                    $"El estado declarado '{declared}' no es reconocido. Se usa el estado calculado: {computed}.");
                break;
            case var status when status != computed:
                yield return ImportFinding.Warning(
                    ImportIssueCodes.DeclaredStatusMismatch,
                    $"El archivo declara el estado '{declared}', pero según sus notas de crédito corresponde {computed}. " +
                    "Se usa el estado calculado.");
                break;
        }
    }

    private static InvoiceStatus? Parse(string value) => value.ToLowerInvariant() switch
    {
        "issued" => InvoiceStatus.Issued,
        "partial" => InvoiceStatus.Partial,
        "canceled" or "cancelled" => InvoiceStatus.Cancelled,
        _ => null,
    };
}
