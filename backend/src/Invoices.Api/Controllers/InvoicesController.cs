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
    [EndpointSummary("Busca facturas activas")]
    [EndpointDescription(
        "Filtra por número, estado de factura (Issued, Partial, Cancelled) y estado de pago (Pending, Overdue, Paid), " +
        "solos o combinados. Consulta la vista vw_invoice_summary, así que las facturas inconsistentes no aparecen. " +
        "El estado de pago se calcula a la fecha asOf (yyyy-MM-dd); si se omite, a la fecha de hoy en Chile.")]
    [ProducesResponseType<PagedResult<InvoiceListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson)]
    public async Task<IActionResult> Search([FromQuery] InvoiceSearchQuery query, CancellationToken cancellationToken) =>
        (await queries.SearchAsync(query.ToCriteria(), query.AsOf, cancellationToken)).ToActionResult(this);

    [HttpGet("{invoiceNumber:int}", Name = "GetInvoice")]
    [EndpointSummary("Obtiene el detalle de una factura activa")]
    [EndpointDescription(
        "Incluye cliente, productos, notas de crédito, pago, saldo pendiente, estados calculados y observaciones de la " +
        "importación. Una factura inconsistente devuelve 404 porque no forma parte del sistema activo.")]
    [ProducesResponseType<InvoiceDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson)]
    public async Task<IActionResult> GetByNumber(
        int invoiceNumber,
        [FromQuery] DateOnly? asOf,
        CancellationToken cancellationToken) =>
        (await queries.GetByNumberAsync(invoiceNumber, asOf, cancellationToken)).ToActionResult(this);

    [HttpPost("{invoiceNumber:int}/credit-notes")]
    [EndpointSummary("Agrega una nota de crédito a una factura")]
    [EndpointDescription(
        "El monto es un entero en pesos, mayor a 0, que no puede superar el saldo pendiente (total menos notas de " +
        "crédito). El servidor asigna la fecha (hoy) y el número. Responde 422 si el monto supera el saldo o si la " +
        "factura ya no tiene saldo.")]
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
