using Invoices.Api.Common;
using Invoices.Api.Contracts;
using Invoices.Application.Common;
using Invoices.Application.CreditNotes;
using Invoices.Application.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invoices.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public sealed class InvoicesController(IInvoiceQueryService queries, ICreditNoteService creditNotes) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<InvoiceListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson)]
    public async Task<IActionResult> Search([FromQuery] InvoiceSearchQuery query, CancellationToken cancellationToken) =>
        (await queries.SearchAsync(query.ToCriteria(), query.AsOf, cancellationToken)).ToActionResult(this);

    [HttpGet("{invoiceNumber:int}", Name = "GetInvoice")]
    [ProducesResponseType<InvoiceDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson)]
    public async Task<IActionResult> GetByNumber(
        int invoiceNumber,
        [FromQuery] DateOnly? asOf,
        CancellationToken cancellationToken) =>
        (await queries.GetByNumberAsync(invoiceNumber, asOf, cancellationToken)).ToActionResult(this);

    [HttpPost("{invoiceNumber:int}/credit-notes")]
    [Consumes("application/json")]
    [ProducesResponseType<CreditNoteCreatedDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, ProblemJson)]
    public async Task<IActionResult> AddCreditNote(
        int invoiceNumber,
        [FromBody] CreateCreditNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await creditNotes.AddAsync(invoiceNumber, request.Amount ?? 0, cancellationToken);
        return result.IsSuccess
            ? CreatedAtRoute("GetInvoice", new { invoiceNumber }, result.Value)
            : result.Error.ToProblem(this);
    }
}
