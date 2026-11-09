using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Invoices.IntegrationTests.Infrastructure;

internal static class HttpAssertions
{
    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == expected, $"Se esperaba {(int)expected} y llegó {(int)response.StatusCode}: {body}");
        return JsonSerializer.Deserialize<T>(body, TestJson.Options)!;
    }

    public static async Task<JsonElement> ReadProblemAsync(this HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
