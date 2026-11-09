using Invoices.Domain.Customers;

namespace Invoices.UnitTests.Domain;

public sealed class ChileanRunTests
{
    [Theory]
    [InlineData("13075795-2")]
    [InlineData("24753685-k")]
    [InlineData("24753685-K")]
    [InlineData("12.345.678-5")]
    [InlineData(" 99265482-1 ")]
    public void Acepta_RUN_con_digito_verificador_correcto(string run) =>
        Assert.True(ChileanRun.IsValid(run));

    [Theory]
    [InlineData("9G13WC4RU71")]
    [InlineData("13075795-3")]
    [InlineData("13075795")]
    [InlineData("-2")]
    [InlineData("123456789-0")]
    [InlineData("")]
    [InlineData(null)]
    public void Rechaza_RUN_invalido(string? run) =>
        Assert.False(ChileanRun.IsValid(run));

    [Fact]
    public void Normaliza_puntos_espacios_y_k_minuscula() =>
        Assert.Equal("24753685-K", ChileanRun.Normalize(" 24.753.685-k "));
}
