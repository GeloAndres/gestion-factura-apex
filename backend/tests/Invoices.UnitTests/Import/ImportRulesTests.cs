using Invoices.Application.Import.Rules;
using Invoices.Application.Import.Source;
using Invoices.Domain.Invoices;

namespace Invoices.UnitTests.Import;

public sealed class ImportRulesTests
{
    [Fact]
    public void Totales_que_cuadran_no_generan_hallazgos() =>
        Assert.Empty(new TotalsConsistencyRule().Evaluate(ImportedInvoiceBuilder.Build()));

    [Fact]
    public void Una_diferencia_de_un_peso_ya_es_inconsistencia()
    {
        var invoice = ImportedInvoiceBuilder.Build(total: 999);

        var finding = Assert.Single(new TotalsConsistencyRule().Evaluate(invoice));

        Assert.Equal(ImportIssueCodes.TotalMismatch, finding.Code);
        Assert.Equal(ImportIssueSeverity.Inconsistency, finding.Severity);
    }

    [Fact]
    public void Subtotal_distinto_de_precio_por_cantidad_es_advertencia()
    {
        var invoice = ImportedInvoiceBuilder.Build(
            total: 254_735,
            lines: [new ImportedLine("Lamb - Racks, Frenched", 3_919, 30, 254_735)]);

        var finding = Assert.Single(new LineSubtotalRule().Evaluate(invoice));

        Assert.Equal(ImportIssueSeverity.Warning, finding.Severity);
        Assert.Contains("$117.570", finding.Message);
    }

    [Fact]
    public void Vencimiento_que_no_cuadra_con_days_to_due_es_advertencia()
    {
        var invoice = ImportedInvoiceBuilder.Build(daysToDue: 75, paymentDueDate: new DateOnly(2025, 7, 31));

        var finding = Assert.Single(new DueDateRule().Evaluate(invoice));

        Assert.Equal(ImportIssueCodes.DueDateMismatch, finding.Code);
    }

    [Fact]
    public void RUN_invalido_es_advertencia() =>
        Assert.Equal(
            ImportIssueCodes.InvalidRun,
            Assert.Single(new CustomerRunRule().Evaluate(ImportedInvoiceBuilder.Build(run: "9G13WC4RU71"))).Code);

    [Fact]
    public void Canceled_sin_notas_de_credito_no_coincide_con_el_estado_calculado()
    {
        var invoice = ImportedInvoiceBuilder.Build(declaredStatus: "canceled");

        var finding = Assert.Single(new DeclaredInvoiceStatusRule().Evaluate(invoice));

        Assert.Equal(ImportIssueCodes.DeclaredStatusMismatch, finding.Code);
        Assert.Contains("Issued", finding.Message);
    }

    [Theory]
    [InlineData("canceled")]
    [InlineData("Cancelled")]
    public void Canceled_con_notas_de_credito_por_el_total_es_coherente(string declared)
    {
        var invoice = ImportedInvoiceBuilder.Build(
            declaredStatus: declared,
            creditNotes: [new ImportedCreditNote(1, ImportedInvoiceBuilder.InvoiceDate, 1_000)]);

        Assert.Empty(new DeclaredInvoiceStatusRule().Evaluate(invoice));
    }

    [Fact]
    public void Estado_declarado_desconocido_es_advertencia() =>
        Assert.Equal(
            ImportIssueCodes.UnknownDeclaredStatus,
            Assert.Single(new DeclaredInvoiceStatusRule().Evaluate(ImportedInvoiceBuilder.Build(declaredStatus: "draft"))).Code);

    [Fact]
    public void Paid_sin_fecha_de_pago_es_advertencia() =>
        Assert.Single(new DeclaredPaymentStatusRule().Evaluate(ImportedInvoiceBuilder.Build(declaredPaymentStatus: "Paid")));

    [Fact]
    public void Pending_con_pago_registrado_es_advertencia() =>
        Assert.Single(new DeclaredPaymentStatusRule().Evaluate(ImportedInvoiceBuilder.Build(
            declaredPaymentStatus: "Pending",
            payment: new ImportedPayment("PayPal", ImportedInvoiceBuilder.InvoiceDate))));

    [Fact]
    public void Paid_con_pago_registrado_es_coherente() =>
        Assert.Empty(new DeclaredPaymentStatusRule().Evaluate(ImportedInvoiceBuilder.Build(
            declaredPaymentStatus: "Paid",
            payment: new ImportedPayment("PayPal", ImportedInvoiceBuilder.InvoiceDate))));

    [Fact]
    public void Notas_de_credito_que_superan_el_total_son_advertencia() =>
        Assert.Single(new CreditNotesWithinTotalRule().Evaluate(ImportedInvoiceBuilder.Build(
            declaredStatus: null,
            creditNotes: [new ImportedCreditNote(1, ImportedInvoiceBuilder.InvoiceDate, 1_001)])));
}
