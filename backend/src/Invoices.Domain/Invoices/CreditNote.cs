namespace Invoices.Domain.Invoices;

public sealed class CreditNote
{
    private CreditNote()
    {
    }

    internal CreditNote(int number, DateOnly date, long amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        Number = number;
        Date = date;
        Amount = amount;
    }

    public int Id { get; private set; }

    public int InvoiceId { get; private set; }

    public int Number { get; private set; }

    public DateOnly Date { get; private set; }

    public long Amount { get; private set; }
}
