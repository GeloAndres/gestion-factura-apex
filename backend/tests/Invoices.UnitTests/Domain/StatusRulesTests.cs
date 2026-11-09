using Invoices.Domain.Invoices;

namespace Invoices.UnitTests.Domain;

public sealed class StatusRulesTests
{
    [Theory]
    [InlineData(100_000, 0, InvoiceStatus.Issued)]
    [InlineData(100_000, 40_000, InvoiceStatus.Partial)]
    [InlineData(100_000, 99_999, InvoiceStatus.Partial)]
    [InlineData(100_000, 100_000, InvoiceStatus.Cancelled)]
    [InlineData(100_000, 120_000, InvoiceStatus.Cancelled)]
    public void Estado_de_factura_segun_notas_de_credito(long total, long creditNotes, InvoiceStatus expected) =>
        Assert.Equal(expected, InvoiceStatusRules.Resolve(total, creditNotes));

    private static readonly DateOnly DueDate = new(2025, 3, 20);

    [Fact]
    public void Pago_registrado_es_Paid_aunque_este_vencida() =>
        Assert.Equal(
            PaymentStatus.Paid,
            PaymentStatusRules.Resolve(new DateOnly(2025, 4, 1), DueDate, new DateOnly(2026, 1, 1)));

    [Fact]
    public void El_dia_del_vencimiento_sigue_Pending() =>
        Assert.Equal(PaymentStatus.Pending, PaymentStatusRules.Resolve(null, DueDate, DueDate));

    [Fact]
    public void Un_dia_despues_del_vencimiento_es_Overdue() =>
        Assert.Equal(PaymentStatus.Overdue, PaymentStatusRules.Resolve(null, DueDate, DueDate.AddDays(1)));

    [Fact]
    public void Antes_del_vencimiento_es_Pending() =>
        Assert.Equal(PaymentStatus.Pending, PaymentStatusRules.Resolve(null, DueDate, DueDate.AddDays(-10)));

    [Theory]
    [InlineData(31, 31)]
    [InlineData(0, 0)]
    [InlineData(-5, 0)]
    public void Dias_vencidos(int daysAfterDue, int expected) =>
        Assert.Equal(expected, PaymentStatusRules.DaysOverdue(DueDate, DueDate.AddDays(daysAfterDue)));
}
