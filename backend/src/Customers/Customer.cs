namespace Invoices.Domain.Customers;

/// <summary>Cliente de una factura. Se identifica por su RUN.</summary>
public sealed class Customer
{
    private Customer()
    {
    } // EF Core

    public Customer(string run, string name, string? email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(run);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Run = ChileanRun.Normalize(run);
        Name = name.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    public int Id { get; private set; }

    public string Run { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Email { get; private set; }
}
