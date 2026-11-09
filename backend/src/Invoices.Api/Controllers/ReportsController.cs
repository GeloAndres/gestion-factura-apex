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
    [ProducesResponseType<OverdueInvoicesReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson)]
    public async Task<IActionResult> GetOverdueInvoices(
        [FromQuery, Range(0, 3650)] int minDaysOverdue = ReportService.DefaultMinDaysOverdue,
        [FromQuery] DateOnly? asOf = null,
        CancellationToken cancellationToken = default) =>
        (await reports.GetOverdueInvoicesAsync(minDaysOverdue, asOf, cancellationToken)).ToActionResult(this);

    [HttpGet("payment-status-summary")]
    [ProducesResponseType<PaymentStatusSummaryDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentStatusSummaryDto>> GetPaymentStatusSummary(
        [FromQuery] DateOnly? asOf,
        CancellationToken cancellationToken) =>
        Ok(await reports.GetPaymentStatusSummaryAsync(asOf, cancellationToken));

    [HttpGet("inconsistent-invoices")]
    [ProducesResponseType<IReadOnlyList<InconsistentInvoiceDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InconsistentInvoiceDto>>> GetInconsistentInvoices(
        CancellationToken cancellationToken) =>
        Ok(await reports.GetInconsistentInvoicesAsync(cancellationToken));
}
