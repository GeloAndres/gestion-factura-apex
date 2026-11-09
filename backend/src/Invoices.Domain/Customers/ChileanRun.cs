namespace Invoices.Domain.Customers;

public static class ChileanRun
{
    public static string Normalize(string run) =>
        run.Trim().Replace(".", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    public static bool IsValid(string? run)
    {
        if (string.IsNullOrWhiteSpace(run))
        {
            return false;
        }

        var value = Normalize(run);
        var dash = value.IndexOf('-', StringComparison.Ordinal);
        if (dash < 1 || dash != value.Length - 2)
        {
            return false;
        }

        var body = value[..dash];
        if (body.Length > 8 || !body.All(char.IsAsciiDigit))
        {
            return false;
        }

        return ComputeCheckDigit(body) == value[^1];
    }

    public static char ComputeCheckDigit(string body)
    {
        var sum = 0;
        var factor = 2;
        for (var i = body.Length - 1; i >= 0; i--)
        {
            sum += (body[i] - '0') * factor;
            factor = factor == 7 ? 2 : factor + 1;
        }

        return (11 - (sum % 11)) switch
        {
            11 => '0',
            10 => 'K',
            var digit => (char)('0' + digit),
        };
    }
}
