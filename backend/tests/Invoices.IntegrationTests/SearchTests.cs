using System.Net;
using Invoices.Application.Common;
using Invoices.Application.Invoices;
using Invoices.Domain.Invoices;
using Invoices.IntegrationTests.Infrastructure;

namespace Invoices.IntegrationTests;

public sealed class SearchTests(InvoicesApiFactory factory) : IClassFixture<InvoicesApiFactory>
{
    private async Task<PagedResult<InvoiceListItemDto>> SearchAsync(string query)
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        return await (await client.GetAsync($"/api/invoices?{query}")).ReadAsync<PagedResult<InvoiceListItemDto>>();
    }

    private static int[] Numbers(PagedResult<InvoiceListItemDto> page) =>
        page.Items.Select(item => item.InvoiceNumber).ToArray();

    [Fact]
    public async Task El_sistema_activo_tiene_las_48_facturas_consistentes()
    {
        var page = await SearchAsync("pageSize=100");

        Assert.Equal(48, page.TotalItems);
        Assert.DoesNotContain(41, Numbers(page));
        Assert.DoesNotContain(48, Numbers(page));
    }

    [Fact]
    public async Task Busca_por_numero_y_calcula_el_estado_en_vez_de_copiarlo()
    {
        var item = Assert.Single((await SearchAsync("number=30")).Items);

        Assert.Equal(InvoiceStatus.Issued, item.InvoiceStatus);
    }

    [Theory]
    [InlineData("Partial", new[] { 2, 13, 21, 28, 32, 34 })]
    [InlineData("Cancelled", new[] { 22 })]
    public async Task Filtra_por_estado_de_factura(string status, int[] expected) =>
        Assert.Equal(expected, Numbers(await SearchAsync($"invoiceStatus={status}&pageSize=100")));

    [Fact]
    public async Task Filtra_por_estado_de_pago_a_la_fecha_de_hoy() =>
        Assert.Equal(25, (await SearchAsync("paymentStatus=Pending&pageSize=100")).TotalItems);

    [Fact]
    public async Task La_fecha_de_referencia_cambia_el_estado_de_pago()
    {
        Assert.Equal(0, (await SearchAsync("paymentStatus=Pending&asOf=2026-11-07")).TotalItems);
        Assert.Equal(35, (await SearchAsync("paymentStatus=Overdue&asOf=2026-11-07")).TotalItems);
    }

    [Fact]
    public async Task Combina_filtros() =>
        Assert.Equal(new[] { 2, 32 }, Numbers(await SearchAsync("invoiceStatus=Partial&paymentStatus=Paid")));

    [Fact]
    public async Task Pagina_los_resultados()
    {
        var page = await SearchAsync("page=5&pageSize=10");

        Assert.Equal(8, page.Items.Count);
        Assert.Equal(5, page.TotalPages);
    }

    [Theory]
    [InlineData("invoiceStatus=Draft")]
    [InlineData("paymentStatus=Late")]
    [InlineData("number=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    [InlineData("asOf=2025-13-01")]
    public async Task Parametros_invalidos_devuelven_400(string query)
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        await (await client.GetAsync($"/api/invoices?{query}")).ReadProblemAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Parametro_mal_escrito_se_explica_en_espanol()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var problem = await (await client.GetAsync("/api/invoices?asOf=2025-13-01")).ReadProblemAsync(HttpStatusCode.BadRequest);

        Assert.Equal("Solicitud inválida", problem.GetProperty("title").GetString());
        Assert.Equal(
            "El valor '2025-13-01' no es válido para AsOf.",
            problem.GetProperty("errors").GetProperty("AsOf")[0].GetString());
    }

    [Fact]
    public async Task El_detalle_incluye_lineas_y_observaciones_de_importacion()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var detail = await (await client.GetAsync("/api/invoices/30")).ReadAsync<InvoiceDetailDto>();

        Assert.Equal(InvoiceStatus.Issued, detail.InvoiceStatus);
        Assert.Equal(PaymentStatus.Pending, detail.PaymentStatus);
        Assert.Null(detail.DaysOverdue);
        Assert.Equal(InvoicesApiFactory.Today, detail.AsOf);
        Assert.Single(detail.Lines);
        Assert.Null(detail.Payment);
        Assert.Contains(detail.ImportIssues, issue => issue.Code == "DECLARED_STATUS_MISMATCH");
        Assert.Contains(detail.ImportIssues, issue => issue.Code == "DUE_DATE_MISMATCH");
    }

    [Fact]
    public async Task El_detalle_de_una_factura_vencida_informa_los_dias_de_atraso()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var detail = await (await client.GetAsync("/api/invoices/39")).ReadAsync<InvoiceDetailDto>();

        Assert.Equal(PaymentStatus.Overdue, detail.PaymentStatus);
        Assert.Equal(78, detail.DaysOverdue);
    }

    [Fact]
    public async Task El_dia_del_vencimiento_la_factura_sigue_Pending()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var onDueDate = await (await client.GetAsync("/api/invoices/9?asOf=2025-03-20")).ReadAsync<InvoiceDetailDto>();
        var dayAfter = await (await client.GetAsync("/api/invoices/9?asOf=2025-03-21")).ReadAsync<InvoiceDetailDto>();

        Assert.Equal(PaymentStatus.Pending, onDueDate.PaymentStatus);
        Assert.Equal(PaymentStatus.Overdue, dayAfter.PaymentStatus);
        Assert.Equal(1, dayAfter.DaysOverdue);
    }

    [Fact]
    public async Task Una_factura_inconsistente_no_esta_en_el_sistema_activo()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var problem = await (await client.GetAsync("/api/invoices/41")).ReadProblemAsync(HttpStatusCode.NotFound);

        Assert.Equal("invoice.not_found", problem.GetProperty("code").GetString());
    }
}
