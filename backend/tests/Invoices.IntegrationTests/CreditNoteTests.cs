using System.Net;
using System.Net.Http.Json;
using Invoices.Application.CreditNotes;
using Invoices.Application.Invoices;
using Invoices.Domain.Invoices;
using Invoices.IntegrationTests.Infrastructure;

namespace Invoices.IntegrationTests;

public sealed class CreditNoteTests(InvoicesApiFactory factory) : IClassFixture<InvoicesApiFactory>
{
    private async Task<HttpResponseMessage> PostAsync(int invoiceNumber, object body)
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        return await client.PostAsJsonAsync($"/api/invoices/{invoiceNumber}/credit-notes", body);
    }

    [Fact]
    public async Task Nota_parcial_crea_201_con_fecha_de_hoy_y_estado_Partial()
    {
        var response = await PostAsync(1, new { amount = 15_019 });

        var created = await response.ReadAsync<CreditNoteCreatedDto>(HttpStatusCode.Created);
        Assert.Equal("/api/invoices/1", response.Headers.Location?.AbsolutePath);
        Assert.Equal(InvoicesApiFactory.Today, created.Date);
        Assert.True(created.Number > 9_027, "El número de NC debe ser correlativo al mayor existente.");
        Assert.Equal(250_000, created.PendingBalance);
        Assert.Equal(InvoiceStatus.Partial, created.InvoiceStatus);
    }

    [Fact]
    public async Task Nota_por_el_saldo_exacto_anula_la_factura()
    {
        var created = await (await PostAsync(21, new { amount = 60_814 })).ReadAsync<CreditNoteCreatedDto>(HttpStatusCode.Created);
        Assert.Equal(InvoiceStatus.Cancelled, created.InvoiceStatus);
        Assert.Equal(0, created.PendingBalance);

        var client = await factory.CreateAuthenticatedClientAsync();
        var detail = await (await client.GetAsync("/api/invoices/21")).ReadAsync<InvoiceDetailDto>();
        Assert.Equal(InvoiceStatus.Cancelled, detail.InvoiceStatus);
        Assert.Equal(2, detail.CreditNotes.Count);

        var problem = await (await PostAsync(21, new { amount = 1 })).ReadProblemAsync(HttpStatusCode.UnprocessableEntity);
        Assert.Equal("credit_note.no_pending_balance", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Monto_mayor_al_saldo_devuelve_422_y_no_guarda_nada()
    {
        var problem = await (await PostAsync(13, new { amount = 80_819 })).ReadProblemAsync(HttpStatusCode.UnprocessableEntity);
        Assert.Equal("credit_note.exceeds_pending_balance", problem.GetProperty("code").GetString());

        var client = await factory.CreateAuthenticatedClientAsync();
        var detail = await (await client.GetAsync("/api/invoices/13")).ReadAsync<InvoiceDetailDto>();
        Assert.Single(detail.CreditNotes);
        Assert.Equal(80_818, detail.PendingBalance);
    }

    [Fact]
    public async Task Una_factura_pagada_igual_acepta_notas_de_credito_hasta_su_saldo()
    {
        var created = await (await PostAsync(32, new { amount = 945 })).ReadAsync<CreditNoteCreatedDto>(HttpStatusCode.Created);

        Assert.Equal(49_000, created.PendingBalance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Monto_no_positivo_devuelve_400(long amount) =>
        await (await PostAsync(2, new { amount })).ReadProblemAsync(HttpStatusCode.BadRequest);

    [Fact]
    public async Task Monto_con_decimales_o_ausente_devuelve_400()
    {
        await (await PostAsync(2, new { amount = 10.5 })).ReadProblemAsync(HttpStatusCode.BadRequest);
        await (await PostAsync(2, new { })).ReadProblemAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Monto_con_decimales_explica_en_espanol_que_debe_ser_entero()
    {
        var problem = await (await PostAsync(2, new { amount = 10.5 })).ReadProblemAsync(HttpStatusCode.BadRequest);

        Assert.Equal("Solicitud inválida", problem.GetProperty("title").GetString());
        var error = Assert.Single(problem.GetProperty("errors").EnumerateObject());
        Assert.Equal("Amount", error.Name);
        Assert.Equal("amount debe ser un número entero, sin decimales.", error.Value[0].GetString());
    }

    [Theory]
    [InlineData(41)]
    [InlineData(999)]
    public async Task Factura_inconsistente_o_inexistente_devuelve_404(int invoiceNumber) =>
        await (await PostAsync(invoiceNumber, new { amount = 1 })).ReadProblemAsync(HttpStatusCode.NotFound);

    [Fact]
    public async Task Sin_token_devuelve_401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/invoices/2/credit-notes", new { amount = 1 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
