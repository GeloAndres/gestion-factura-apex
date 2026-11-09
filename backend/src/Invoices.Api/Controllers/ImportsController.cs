using Invoices.Api.Common;
using Invoices.Api.Contracts;
using Invoices.Application.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invoices.Api.Controllers;

[ApiController]
[Route("api/imports")]
[Authorize]
public sealed class ImportsController(IInvoiceImportService importService) : ControllerBase
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    [HttpPost]
    [EndpointSummary("Importa facturas desde un archivo JSON")]
    [EndpointDescription(
        "El archivo va en el campo file (multipart/form-data, máximo 5 MB) y tiene el mismo formato que " +
        "bd_exam.json. Omite los invoice_number que ya existen. Las facturas inconsistentes se guardan, pero quedan " +
        "fuera del sistema activo.")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSizeBytes + (64 * 1024))]
    [ProducesResponseType<ImportResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson)]
    public async Task<IActionResult> Import([FromForm] ImportFileRequest request, CancellationToken cancellationToken)
    {
        var file = request.File!;
        if (!string.Equals(Path.GetExtension(file.FileName), ".json", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(request.File), "El archivo debe tener extensión .json.");
        }

        if (file.Length == 0)
        {
            ModelState.AddModelError(nameof(request.File), "El archivo está vacío.");
        }
        else if (file.Length > MaxFileSizeBytes)
        {
            ModelState.AddModelError(nameof(request.File), "El archivo no puede superar 5 MB.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await using var stream = file.OpenReadStream();
        var result = await importService.ImportAsync(stream, Path.GetFileName(file.FileName), cancellationToken);
        return result.ToActionResult(this);
    }
}
