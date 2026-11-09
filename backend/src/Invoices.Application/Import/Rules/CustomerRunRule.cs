using Invoices.Application.Import.Source;
using Invoices.Domain.Customers;

namespace Invoices.Application.Import.Rules;

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
