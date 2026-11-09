using System.Net;
using Invoices.Application.Reports;
using Invoices.Domain.Invoices;
using Invoices.IntegrationTests.Infrastructure;

namespace Invoices.IntegrationTests;

public sealed class ReportTests(InvoicesApiFactory factory) : IClassFixture<InvoicesApiFactory>
{
    private async Task<T> GetAsync<T>(string url)
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        return await (await client.GetAsync(url)).ReadAsync<T>();
    }

    private static (int Count, decimal Percentage) For(PaymentStatusSummaryDto summary, PaymentStatus status)
    {
        var item = summary.Items.Single(candidate => candidate.Status == status);
        return (item.Count, item.Percentage);
    }

    [Fact]
    public async Task Resumen_por_estado_de_pago_a_la_fecha_de_hoy()
    {
        var summary = await GetAsync<PaymentStatusSummaryDto>("/api/reports/payment-status-summary");

        Assert.Equal(InvoicesApiFactory.Today, summary.AsOf);
        Assert.Equal(48, summary.TotalInvoices);
        Assert.Equal((13, 27.1m), For(summary, PaymentStatus.Paid));
        Assert.Equal((25, 52.1m), For(summary, PaymentStatus.Pending));
        Assert.Equal((10, 20.8m), For(summary, PaymentStatus.Overdue));
    }

    [Fact]
    public async Task Resumen_con_fecha_de_referencia_actual_no_tiene_pendientes()
    {
        var summary = await GetAsync<PaymentStatusSummaryDto>("/api/reports/payment-status-summary?asOf=2026-11-07");

        Assert.Equal((13, 27.1m), For(summary, PaymentStatus.Paid));
        Assert.Equal((0, 0m), For(summary, PaymentStatus.Pending));
        Assert.Equal((35, 72.9m), For(summary, PaymentStatus.Overdue));
    }

    [Fact]
    public async Task Vencidas_por_mas_de_30_dias_sin_pago_ni_notas_de_credito()
    {
        var report = await GetAsync<OverdueInvoicesReportDto>("/api/reports/overdue-invoices");

        Assert.Equal(new[] { 39, 45, 36 }, report.Items.Select(item => item.InvoiceNumber).ToArray());
        Assert.Equal(new[] { 78, 64, 58 }, report.Items.Select(item => item.DaysOverdue).ToArray());
        Assert.Equal(963_143, report.TotalAmount);
    }

    [Fact]
    public async Task Vencidas_al_dia_de_hoy_real_incluye_30_facturas()
    {
        var report = await GetAsync<OverdueInvoicesReportDto>("/api/reports/overdue-invoices?asOf=2026-11-07");

        Assert.Equal(30, report.Count);
        Assert.DoesNotContain(report.Items, item => item.InvoiceNumber is 13 or 21 or 22 or 28 or 34);
    }

    [Fact]
    public async Task Dias_negativos_devuelven_400()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        await (await client.GetAsync("/api/reports/overdue-invoices?minDaysOverdue=-1"))
            .ReadProblemAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Reporte_de_inconsistentes_muestra_la_diferencia_y_la_linea_erronea()
    {
        var report = await GetAsync<List<InconsistentInvoiceDto>>("/api/reports/inconsistent-invoices");

        Assert.Equal(new[] { 41, 48 }, report.Select(item => item.InvoiceNumber).ToArray());

        var invoice41 = report[0];
        Assert.Equal(691_171, invoice41.DeclaredTotal);
        Assert.Equal(828_336, invoice41.LinesTotal);
        Assert.Equal(-137_165, invoice41.Difference);
        var wrongLine = Assert.Single(invoice41.Lines, line => !line.SubtotalMatches);
        Assert.Equal(117_570, wrongLine.ExpectedSubtotal);

        Assert.Equal(-6, report[1].Difference);
    }
}
