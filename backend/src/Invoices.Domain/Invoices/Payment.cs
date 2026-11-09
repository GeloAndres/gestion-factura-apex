namespace Invoices.Domain.Invoices;

public sealed class Payment
{
    private Payment()
    {
    }

    internal Payment(string? method, DateOnly paymentDate)
    {
        Method = string.IsNullOrWhiteSpace(method) ? null : method.Trim();
        PaymentDate = paymentDate;
    }

    public string? Method { get; private set; }

    public DateOnly PaymentDate { get; private set; }
}
