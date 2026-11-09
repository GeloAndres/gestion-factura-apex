using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Invoices.Application.Import;
using Invoices.Application.Invoices;
using Invoices.Application.Reports;
using Invoices.IntegrationTests.Infrastructure;

namespace Invoices.IntegrationTests;

public sealed class ImportTests(InvoicesApiFactory factory) : IClassFixture<InvoicesApiFactory>
{
    private async Task<HttpResponseMessage> UploadAsync(string fileName, string content)
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        file.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        form.Add(file, "file", fileName);
        return await client.PostAsync("/api/imports", form);
    }

    private static string Invoice(int number, long total, long subtotal, string run = "13075795-2") => $$$"""
        {"invoice_number":{{{number}}},"invoice_date":"2025-02-01","invoice_status":"issued","total_amount":{{{total}}},
         "days_to_due":30,"payment_due_date":"2025-03-03","payment_status":"Pending",
         "invoice_detail":[{"product_name":"Producto","unit_price":{{{subtotal}}},"quantity":1,"subtotal":{{{subtotal}}}}],
         "invoice_payment":{"payment_method":null,"payment_date":null},"invoice_credit_note":[],
         "customer":{"customer_run":"{{{run}}}","customer_name":"Cliente nuevo","customer_email":null}}
        """;

    [Fact]
    public async Task Volver_a_importar_el_mismo_archivo_no_duplica_facturas()
    {
        var result = await (await UploadAsync("bd_exam.json", await File.ReadAllTextAsync(InvoicesApiFactory.SampleFilePath)))
            .ReadAsync<ImportResultDto>();

        Assert.Equal(50, result.TotalRecords);
        Assert.Equal(0, result.Imported);
        Assert.Equal(50, result.Skipped.Count);
        Assert.All(result.Skipped, skipped => Assert.Contains("Ya existe", skipped.Reason));
    }

    [Fact]
    public async Task Importa_nuevas_separa_inconsistentes_y_omite_duplicados_e_incompletos()
    {
        var json = $$"""
            {"invoices":[
              {{Invoice(100, 5_000, 5_000)}},
              {{Invoice(101, 5_000, 4_000)}},
              {{Invoice(1, 1_000, 1_000)}},
              {{Invoice(100, 7_000, 7_000)}},
              {"invoice_number":102}
            ]}
            """;

        var result = await (await UploadAsync("nuevas.json", json)).ReadAsync<ImportResultDto>();

        Assert.Equal(5, result.TotalRecords);
        Assert.Equal(2, result.Imported);
        Assert.Equal(new[] { 101 }, result.InconsistentInvoiceNumbers.ToArray());
        Assert.Equal(new[] { 3, 4, 5 }, result.Skipped.Select(skipped => skipped.Position).ToArray());

        var client = await factory.CreateAuthenticatedClientAsync();
        var detail = await (await client.GetAsync("/api/invoices/100")).ReadAsync<InvoiceDetailDto>();
        Assert.Equal(5_000, detail.TotalAmount);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/invoices/101")).StatusCode);

        var inconsistent = await (await client.GetAsync("/api/reports/inconsistent-invoices")).ReadAsync<List<InconsistentInvoiceDto>>();
        Assert.Contains(inconsistent, item => item.InvoiceNumber == 101 && item.Difference == 1_000);
    }

    [Fact]
    public async Task JSON_invalido_devuelve_400()
    {
        var problem = await (await UploadAsync("malo.json", "{ no es json")).ReadProblemAsync(HttpStatusCode.BadRequest);

        Assert.Equal("import.invalid_json", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Archivo_que_no_es_json_devuelve_400() =>
        await (await UploadAsync("facturas.csv", "a,b,c")).ReadProblemAsync(HttpStatusCode.BadRequest);
}
