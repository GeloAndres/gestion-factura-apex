using Invoices.Domain.Common;

namespace Invoices.Application.Import;

public static class ImportErrors
{
    public static readonly Error MissingInvoicesArray = Error.Validation(
        "import.missing_invoices",
        "El JSON debe tener una propiedad \"invoices\" con la lista de facturas.");

    public static Error InvalidJson(string detail) => Error.Validation(
        "import.invalid_json",
        $"El archivo no es un JSON válido o no tiene el formato esperado. Detalle: {detail}");
}
