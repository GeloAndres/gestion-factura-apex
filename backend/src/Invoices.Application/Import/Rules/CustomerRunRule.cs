using Invoices.Application.Import.Source;
using Invoices.Domain.Customers;

namespace Invoices.Application.Import.Rules;

/// <summary>Advierte cuando el RUN del cliente no tiene formato válido o su dígito verificador no cuadra.</summary>
public sealed class CustomerRunRule : IInvoiceImportRule
{
    public IEnumerable<ImportFinding> Evaluate(ImportedInvoice invoice)
    {
        if (!ChileanRun.IsValid(invoice.Customer.Run))
        {
            yield return ImportFinding.Warning(
                ImportIssueCodes.InvalidRun,
                $"El RUN del cliente '{invoice.Customer.Run}' no es válido (formato o dígito verificador).");
        }
    }
}
