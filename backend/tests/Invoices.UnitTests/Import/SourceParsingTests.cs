using System.Text;
using Invoices.Application.Import.Source;

namespace Invoices.UnitTests.Import;

public sealed class SourceParsingTests
{
    private static Stream Json(string content) => new MemoryStream(Encoding.UTF8.GetBytes(content));

    [Fact]
    public async Task Lee_propiedades_en_snake_case()
    {
        const string json = """
            {"invoices":[{"invoice_number":7,"invoice_date":"2024-12-05","invoice_status":"issued","total_amount":371228,
              "days_to_due":84,"payment_due_date":"2025-02-27","payment_status":"Pending",
              "invoice_detail":[{"product_name":"Quail","unit_price":8437,"quantity":44,"subtotal":371228}],
              "invoice_payment":{"payment_method":null,"payment_date":null},"invoice_credit_note":[],
              "customer":{"customer_run":"11826090-2","customer_name":"Timmie","customer_email":"t@ebay.com"}}]}
            """;

        var read = await InvoiceFileReader.ReadAsync(Json(json), CancellationToken.None);

        Assert.True(read.IsSuccess);
        var converted = InvoiceRecordValidator.TryConvert(Assert.Single(read.Value), out var invoice, out var errors);
        Assert.True(converted, string.Join(" ", errors));
        Assert.Equal(7, invoice!.InvoiceNumber);
        Assert.Equal(new DateOnly(2025, 2, 27), invoice.PaymentDueDate);
        Assert.Equal(371_228, Assert.Single(invoice.Lines).Subtotal);
        Assert.Null(invoice.Payment);
        Assert.Equal("11826090-2", invoice.Customer.Run);
    }

    [Fact]
    public async Task JSON_invalido_es_error_de_validacion()
    {
        var read = await InvoiceFileReader.ReadAsync(Json("{ esto no es json"), CancellationToken.None);

        Assert.Equal("import.invalid_json", read.Error?.Code);
    }

    [Fact]
    public async Task Sin_propiedad_invoices_es_error_de_validacion()
    {
        var read = await InvoiceFileReader.ReadAsync(Json("""{"facturas":[]}"""), CancellationToken.None);

        Assert.Equal("import.missing_invoices", read.Error?.Code);
    }

    [Fact]
    public void Registro_incompleto_se_descarta_con_sus_motivos()
    {
        var record = new InvoiceRecord { InvoiceNumber = 0, TotalAmount = -5 };

        Assert.False(InvoiceRecordValidator.TryConvert(record, out _, out var errors));
        Assert.Contains(errors, error => error.Contains("invoice_number"));
        Assert.Contains(errors, error => error.Contains("total_amount"));
        Assert.Contains(errors, error => error.Contains("customer"));
    }

    [Fact]
    public void Sin_days_to_due_se_calcula_desde_las_fechas()
    {
        var record = new InvoiceRecord
        {
            InvoiceNumber = 1,
            InvoiceDate = new DateOnly(2025, 1, 1),
            PaymentDueDate = new DateOnly(2025, 1, 31),
            TotalAmount = 0,
            Customer = new CustomerRecord { CustomerRun = "13075795-2", CustomerName = "Cliente" },
        };

        Assert.True(InvoiceRecordValidator.TryConvert(record, out var invoice, out _));
        Assert.Equal(30, invoice!.DaysToDue);
    }
}
