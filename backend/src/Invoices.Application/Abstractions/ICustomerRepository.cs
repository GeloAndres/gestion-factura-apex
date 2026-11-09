using Invoices.Domain.Customers;

namespace Invoices.Application.Abstractions;

public interface ICustomerRepository
{
    Task<IReadOnlyDictionary<string, Customer>> GetByRunsAsync(
        IReadOnlyCollection<string> runs,
        CancellationToken cancellationToken);

    void Add(Customer customer);
}
