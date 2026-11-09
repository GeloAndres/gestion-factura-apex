using Invoices.Domain.Common;

namespace Invoices.Application.Abstractions;

/// <summary>Unidad de trabajo: confirma los cambios de los repositorios como una sola operación.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Ejecuta la operación dentro de una transacción: confirma si el resultado es exitoso y
    /// revierte si falla o lanza una excepción.
    /// </summary>
    Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken);
}
