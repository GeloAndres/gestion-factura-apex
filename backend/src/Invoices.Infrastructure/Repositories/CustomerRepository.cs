using Invoices.Application.Abstractions;
using Invoices.Domain.Customers;
using Invoices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Invoices.Infrastructure.Repositories;

internal sealed class CustomerRepository(InvoicesDbContext dbContext) : ICustomerRepository
{
    public async Task<IReadOnlyDictionary<string, Customer>> GetByRunsAsync(
        IReadOnlyCollection<string> runs,
        CancellationToken cancellationToken)
    {
        if (runs.Count == 0)
        {
            return new Dictionary<string, Customer>();
        }

        return await dbContext.Customers
            .Where(customer => runs.Contains(customer.Run))
            .ToDictionaryAsync(customer => customer.Run, cancellationToken);
    }

    public void Add(Customer customer) => dbContext.Customers.Add(customer);
}
