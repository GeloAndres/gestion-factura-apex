using System.Text.Json;
using System.Text.Json.Serialization;
using Invoices.Domain.Common;

namespace Invoices.Application.Import.Source;

/// <summary>Lee el archivo JSON de facturas (propiedades en snake_case, como bd_exam.json).</summary>
public static class InvoiceFileReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public static async Task<Result<IReadOnlyList<InvoiceRecord?>>> ReadAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        InvoiceFileModel? file;
        try
        {
            file = await JsonSerializer.DeserializeAsync<InvoiceFileModel>(content, Options, cancellationToken);
        }
        catch (JsonException exception)
        {
            return ImportErrors.InvalidJson(exception.Message);
        }

        if (file?.Invoices is null)
        {
            return ImportErrors.MissingInvoicesArray;
        }

        return Result<IReadOnlyList<InvoiceRecord?>>.Success(file.Invoices);
    }
}
