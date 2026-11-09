using Invoices.Application.Abstractions;
using Invoices.Application.Common;
using Invoices.Domain.Common;
using Invoices.Domain.Invoices;

namespace Invoices.Application.Invoices;

public interface IInvoiceQueryService
{
    Task<Result<PagedResult<InvoiceListItemDto>>> SearchAsync(
        InvoiceSearchCriteria criteria,
        DateOnly? asOf,
        CancellationToken cancellationToken);

    Task<Result<InvoiceDetailDto>> GetByNumberAsync(int invoiceNumber, DateOnly? asOf, CancellationToken cancellationToken);
}

public sealed class InvoiceQueryService(IInvoiceReadRepository repository, IBusinessCalendar calendar)
    : IInvoiceQueryService
{
    public const int MaxPageSize = 100;

    public async Task<Result<PagedResult<InvoiceListItemDto>>> SearchAsync(
        InvoiceSearchCriteria criteria,
        DateOnly? asOf,
        CancellationToken cancellationToken)
    {
        if (criteria.InvoiceNumber is <= 0)
        {
            return Error.Validation("search.invalid_number", "El número de factura debe ser un entero positivo.");
        }

        if (criteria.Page < 1)
        {
            return Error.Validation("search.invalid_page", "La página debe ser mayor o igual a 1.");
        }

        if (criteria.PageSize is < 1 or > MaxPageSize)
        {
            return Error.Validation("search.invalid_page_size", $"El tamaño de página debe estar entre 1 y {MaxPageSize}.");
        }

        return await repository.SearchAsync(criteria, asOf ?? calendar.Today, cancellationToken);
    }

    public async Task<Result<InvoiceDetailDto>> GetByNumberAsync(
        int invoiceNumber,
        DateOnly? asOf,
        CancellationToken cancellationToken)
    {
        var detail = await repository.GetDetailAsync(invoiceNumber, asOf ?? calendar.Today, cancellationToken);
        if (detail is null)
        {
            return InvoiceErrors.NotFound(invoiceNumber);
        }

        return detail;
    }
}
