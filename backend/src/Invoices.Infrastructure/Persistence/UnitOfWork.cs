using Invoices.Application.Abstractions;
using Invoices.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Invoices.Infrastructure.Persistence;

internal sealed class UnitOfWork(InvoicesDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var result = await operation(cancellationToken);
        if (result.IsSuccess)
        {
            await transaction.CommitAsync(cancellationToken);
        }
        else
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }

        return result;
    }
}
