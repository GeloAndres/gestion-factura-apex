using Invoices.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Invoices.Api.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller) =>
        result.IsSuccess ? controller.Ok(result.Value) : result.Error.ToProblem(controller);

    public static IActionResult ToProblem(this Error error, ControllerBase controller)
    {
        var (status, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflicto"),
            ErrorType.BusinessRule => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio no cumplida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno"),
        };

        var problem = controller.ProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode: status,
            title: title,
            detail: error.Message);
        problem.Extensions["code"] = error.Code;

        return new ObjectResult(problem) { StatusCode = status };
    }
}
