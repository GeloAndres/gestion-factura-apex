using System.Net;
using System.Net.Http.Json;
using Invoices.Application.Common;
using Invoices.Application.Invoices;
using Invoices.Infrastructure.Persistence;
using Invoices.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoices.IntegrationTests;

public sealed class AuthAndConsistencyTests(InvoicesApiFactory factory) : IClassFixture<InvoicesApiFactory>
{
    [Fact]
    public async Task Credenciales_incorrectas_devuelven_401()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "incorrecta" });

        var problem = await response.ReadProblemAsync(HttpStatusCode.Unauthorized);
        Assert.Equal("Usuario o contraseña incorrectos.", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Login_sin_contrasena_devuelve_400() =>
        await (await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { username = "admin" }))
            .ReadProblemAsync(HttpStatusCode.BadRequest);

    [Theory]
    [InlineData("/api/invoices")]
    [InlineData("/api/reports/payment-status-summary")]
    [InlineData("/api/reports/inconsistent-invoices")]
    public async Task Los_endpoints_de_negocio_exigen_token(string url) =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync(url)).StatusCode);

    [Fact]
    public async Task Swagger_documenta_la_api()
    {
        var response = await factory.CreateClient().GetAsync("/swagger/v1/swagger.json");
        var document = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/invoices/{invoiceNumber}/credit-notes", document);
        Assert.Contains("\"422\"", document);
    }

    [Fact]
    public async Task La_vista_y_el_dominio_calculan_los_mismos_estados()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InvoicesDbContext>();
        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.CreditNotes)
            .Where(invoice => invoice.IsConsistent)
            .ToDictionaryAsync(invoice => invoice.InvoiceNumber);

        var client = await factory.CreateAuthenticatedClientAsync();
        var page = await (await client.GetAsync("/api/invoices?pageSize=100")).ReadAsync<PagedResult<InvoiceListItemDto>>();

        Assert.Equal(invoices.Count, page.TotalItems);
        foreach (var row in page.Items)
        {
            var invoice = invoices[row.InvoiceNumber];
            Assert.Equal(invoice.Status, row.InvoiceStatus);
            Assert.Equal(invoice.CreditNotesTotal, row.CreditNotesTotal);
            Assert.Equal(invoice.PendingBalance, row.PendingBalance);
            Assert.Equal(invoice.GetPaymentStatus(InvoicesApiFactory.Today), row.PaymentStatus);
        }
    }
}
