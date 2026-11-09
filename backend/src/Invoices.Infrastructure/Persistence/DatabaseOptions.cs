using Microsoft.Data.Sqlite;

namespace Invoices.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string BasePath { get; set; } = AppContext.BaseDirectory;

    public string BuildConnectionString()
    {
        var builder = new SqliteConnectionStringBuilder(ConnectionString);
        var isFile = builder.Mode != SqliteOpenMode.Memory
            && !string.IsNullOrWhiteSpace(builder.DataSource)
            && builder.DataSource != ":memory:";

        if (!isFile)
        {
            return builder.ToString();
        }

        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(Path.Combine(BasePath, builder.DataSource));
        }

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return builder.ToString();
    }
}
