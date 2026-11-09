using Invoices.Application.Import;
using Invoices.Infrastructure;
using Microsoft.Extensions.Options;

namespace Invoices.Api.Startup;

public sealed class ImportOptions
{
    public const string SectionName = "Import";

    public bool RunOnStartup { get; set; } = true;

    public string FilePath { get; set; } = string.Empty;
}

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer));

        await services.MigrateDatabaseAsync();

        var options = services.GetRequiredService<IOptions<ImportOptions>>().Value;
        if (!options.RunOnStartup || string.IsNullOrWhiteSpace(options.FilePath))
        {
            return;
        }

        var path = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, options.FilePath));
        if (!File.Exists(path))
        {
            logger.LogWarning("No se encontró el archivo de facturas {Path}; se omite la carga inicial.", path);
            return;
        }

        await using var stream = File.OpenRead(path);
        var result = await services.GetRequiredService<IInvoiceImportService>()
            .ImportAsync(stream, Path.GetFileName(path), CancellationToken.None);

        if (result.IsFailure)
        {
            logger.LogError("No se pudo cargar {Path}: {Error}", path, result.Error.Message);
        }
    }
}
