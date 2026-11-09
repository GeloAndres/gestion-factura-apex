using System.Globalization;

namespace Invoices.Domain.Common;

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
