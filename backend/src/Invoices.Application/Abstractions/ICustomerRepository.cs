using Invoices.Domain.Customers;

namespace Invoices.Application.Abstractions;

public interface ICustomerRepository
{
    /// <summary>Clientes existentes indexados por RUN normalizado.</summary>
    Task<IReadOnlyDictionary<string, Customer>> GetByRunsAsync(
        IReadOnlyCollection<string> runs,
        CancellationToken cancellationToken);

    void Add(Customer customer);
}
