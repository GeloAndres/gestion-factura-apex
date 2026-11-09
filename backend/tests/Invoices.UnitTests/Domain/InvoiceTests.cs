using Invoices.Domain.Common;
using Invoices.Domain.Customers;
using Invoices.Domain.Invoices;

namespace Invoices.UnitTests.Domain;

public sealed class InvoiceTests
{
    private static readonly DateOnly Today = new(2025, 3, 1);

    private static Invoice NewInvoice(long total = 100_000) =>
        new(1, new DateOnly(2025, 1, 1), 30, new DateOnly(2025, 1, 31), total, new Customer("13075795-2", "Cliente", null));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Rechaza_nota_de_credito_con_monto_no_positivo(long amount)
    {
        var result = NewInvoice().AddCreditNote(1, amount, Today);

        Assert.True(result.IsFailure);
        Assert.Equal(InvoiceErrors.InvalidCreditNoteAmount, result.Error);
    }

    [Fact]
    public void Rechaza_nota_de_credito_que_supera_el_saldo_pendiente()
    {
        var invoice = NewInvoice();
        invoice.RegisterImportedCreditNote(10, Today, 60_000);

        var result = invoice.AddCreditNote(11, 40_001, Today);

        Assert.True(result.IsFailure);
        Assert.Equal("credit_note.exceeds_pending_balance", result.Error.Code);
        Assert.Equal(ErrorType.BusinessRule, result.Error.Type);
        Assert.Equal(40_000, invoice.PendingBalance);
    }

    [Fact]
    public void Nota_de_credito_parcial_deja_la_factura_Partial()
    {
        var invoice = NewInvoice();

        var result = invoice.AddCreditNote(1, 25_000, Today);

        Assert.True(result.IsSuccess);
        Assert.Equal(Today, result.Value.Date);
        Assert.Equal(InvoiceStatus.Partial, invoice.Status);
        Assert.Equal(75_000, invoice.PendingBalance);
    }

    [Fact]
    public void Nota_de_credito_por_el_saldo_exacto_anula_la_factura_y_luego_no_admite_mas()
    {
        var invoice = NewInvoice();
        invoice.RegisterImportedCreditNote(10, Today, 60_000);

        Assert.True(invoice.AddCreditNote(11, 40_000, Today).IsSuccess);
        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        Assert.Equal(0, invoice.PendingBalance);

        var another = invoice.AddCreditNote(12, 1, Today);
        Assert.Equal("credit_note.no_pending_balance", another.Error?.Code);
    }

    [Fact]
    public void El_pago_no_reduce_el_saldo_pendiente()
    {
        var invoice = NewInvoice();
        invoice.RegisterPayment("Apple Pay", new DateOnly(2025, 1, 5));

        Assert.Equal(100_000, invoice.PendingBalance);
        Assert.Equal(PaymentStatus.Paid, invoice.GetPaymentStatus(Today));
        Assert.True(invoice.AddCreditNote(1, 100_000, Today).IsSuccess);
    }

    [Fact]
    public void Una_inconsistencia_de_importacion_saca_la_factura_del_sistema_activo()
    {
        var invoice = NewInvoice();
        invoice.RecordImportIssue("TOTAL_MISMATCH", ImportIssueSeverity.Inconsistency, "No cuadra.");

        Assert.False(invoice.IsConsistent);
        Assert.Equal("invoice.inconsistent", invoice.AddCreditNote(1, 1_000, Today).Error?.Code);
    }

    [Fact]
    public void Una_advertencia_de_importacion_no_cambia_la_consistencia()
    {
        var invoice = NewInvoice();
        invoice.RecordImportIssue("INVALID_RUN", ImportIssueSeverity.Warning, "RUN inválido.");

        Assert.True(invoice.IsConsistent);
    }

    [Theory]
    [InlineData(1_234_567, "$1.234.567")]
    [InlineData(0, "$0")]
    [InlineData(-6, "-$6")]
    public void Formato_de_montos_en_pesos(long amount, string expected) =>
        Assert.Equal(expected, Money.Format(amount));
}
