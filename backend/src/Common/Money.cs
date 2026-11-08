using System.Globalization;

namespace Invoices.Domain.Common;

/// <summary>
/// Formato de montos en pesos chilenos. El CLP no tiene decimales, por eso los montos son <see cref="long"/>.
/// </summary>
public static class Money
{
    private static readonly NumberFormatInfo ChileanFormat = new()
    {
        NumberGroupSeparator = ".",
        NumberGroupSizes = [3],
    };

    public static string Format(long amount) =>
        (amount < 0 ? "-$" : "$") + Math.Abs(amount).ToString("#,0", ChileanFormat);
}
