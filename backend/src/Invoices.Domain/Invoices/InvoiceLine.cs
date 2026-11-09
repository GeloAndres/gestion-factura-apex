namespace Invoices.Domain.Invoices;

public sealed class InvoiceLine
{
    private InvoiceLine()
    {
    }

    internal InvoiceLine(string productName, long unitPrice, int quantity, long subtotal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(subtotal);

        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        Subtotal = subtotal;
    }

    public int Id { get; private set; }

    public int InvoiceId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public long UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public long Subtotal { get; private set; }

    public long ExpectedSubtotal => UnitPrice * Quantity;
}
