using Invoices.Application.Import.Source;

namespace Invoices.Application.Import.Rules;

/// <summary>
/// "Paid" significa pago registrado (con fecha de pago). Advierte cuando el estado de pago declarado
/// contradice los datos de pago del archivo.
/// </summary>
public sealed class DeclaredPaymentStatusRule : IInvoiceImportRule
{
    public IEnumerable<ImportFinding> Evaluate(ImportedInvoice invoice)
    {
        if (string.IsNullOrWhiteSpace(invoice.DeclaredPaymentStatus))
        {
            yield break;
        }

        var declared = invoice.DeclaredPaymentStatus.Trim();
        var declaresPaid = declared.Equals("paid", StringComparison.OrdinalIgnoreCase);

        if (declaresPaid && invoice.Payment is null)
        {
            yield return ImportFinding.Warning(
                ImportIssueCodes.DeclaredPaymentStatusMismatch,
                $"El archivo declara '{declared}', pero no informa fecha de pago. Se considera sin pago registrado.");
        }
        else if (!declaresPaid && invoice.Payment is not null)
        {
            yield return ImportFinding.Warning(
                ImportIssueCodes.DeclaredPaymentStatusMismatch,
                $"El archivo declara '{declared}', pero informa un pago el {invoice.Payment.PaymentDate:yyyy-MM-dd}. Se considera pagada.");
        }
    }
}
