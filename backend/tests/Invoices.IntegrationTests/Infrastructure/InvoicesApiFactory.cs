using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Invoices.Api.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace Invoices.IntegrationTests.Infrastructure;

public sealed class InvoicesApiFactory : WebApplicationFactory<Program>
{
    public static readonly DateOnly Today = new(2025, 3, 1);

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"invoices-tests-{Guid.NewGuid():N}.db");

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2025, 3, 1, 15, 0, 0, TimeSpan.Zero));

    public static string SampleFilePath => Path.Combine(AppContext.BaseDirectory, "TestData", "bd_exam.json");

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "Admin123!" });
        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(TestJson.Options);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Invoices", $"Data Source={_databasePath}");
        builder.UseSetting("Import:RunOnStartup", "true");
        builder.UseSetting("Import:FilePath", SampleFilePath);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        DeleteDatabase();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        DeleteDatabase();
    }

    private void DeleteDatabase()
    {
        SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            File.Delete(_databasePath + suffix);
        }
    }
}

public static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
