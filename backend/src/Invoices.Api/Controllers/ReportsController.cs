using System.ComponentModel.DataAnnotations;
using Invoices.Api.Common;
using Invoices.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invoices.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController(IReportService reports) : ControllerBase
{
    [HttpGet("overdue-invoices")]
    [EndpointSummary("Facturas vencidas hace más de N días, sin pago ni notas de crédito")]
    [EndpointDescription(
        "Solo considera facturas consistentes. Por defecto, minDaysOverdue es 30. Los días se cuentan a la fecha asOf " +
        "(yyyy-MM-dd); si se omite, a la fecha de hoy en Chile.")]
    [ProducesResponseType<OverdueInvoicesReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson)]
    public async Task<IActionResult> GetOverdueInvoices(
        [FromQuery, Range(0, 3650, ErrorMessage = "minDaysOverdue debe estar entre 0 y 3650.")]
        int minDaysOverdue = ReportService.DefaultMinDaysOverdue,
        [FromQuery] DateOnly? asOf = null,
        CancellationToken cancellationToken = default) =>
        (await reports.GetOverdueInvoicesAsync(minDaysOverdue, asOf, cancellationToken)).ToActionResult(this);

    [HttpGet("payment-status-summary")]
    [EndpointSummary("Total y porcentaje de facturas por estado de pago")]
    [EndpointDescription(
        "Cuenta las facturas activas en Paid, Pending y Overdue y calcula su porcentaje sobre el total, a la fecha " +
        "asOf (yyyy-MM-dd); si se omite, a la fecha de hoy en Chile.")]
    [ProducesResponseType<PaymentStatusSummaryDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentStatusSummaryDto>> GetPaymentStatusSummary(
        [FromQuery] DateOnly? asOf,
        CancellationToken cancellationToken) =>
        Ok(await reports.GetPaymentStatusSummaryAsync(asOf, cancellationToken));

    [HttpGet("inconsistent-invoices")]
    [EndpointSummary("Facturas inconsistentes")]
    [EndpointDescription(
        "Facturas cuyo total declarado no coincide con la suma de los subtotales de sus productos. Incluye la " +
        "diferencia y cada línea, marcando las que no cumplen precio × cantidad = subtotal.")]
    [ProducesResponseType<IReadOnlyList<InconsistentInvoiceDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InconsistentInvoiceDto>>> GetInconsistentInvoices(
        CancellationToken cancellationToken) =>
        Ok(await reports.GetInconsistentInvoicesAsync(cancellationToken));
}
