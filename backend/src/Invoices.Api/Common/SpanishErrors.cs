using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.WebUtilities;

namespace Invoices.Api.Common;

public static class SpanishErrors
{
    private const string InvalidValue = "El valor no es válido.";

    private static readonly Dictionary<int, string> Titles = new()
    {
        [StatusCodes.Status400BadRequest] = "Solicitud inválida",
        [StatusCodes.Status401Unauthorized] = "No autorizado",
        [StatusCodes.Status403Forbidden] = "Acceso denegado",
        [StatusCodes.Status404NotFound] = "Recurso no encontrado",
        [StatusCodes.Status405MethodNotAllowed] = "Método no permitido",
        [StatusCodes.Status406NotAcceptable] = "Formato de respuesta no disponible",
        [StatusCodes.Status409Conflict] = "Conflicto",
        [StatusCodes.Status413PayloadTooLarge] = "Solicitud demasiado grande",
        [StatusCodes.Status415UnsupportedMediaType] = "Tipo de contenido no soportado",
        [StatusCodes.Status422UnprocessableEntity] = "Regla de negocio no cumplida",
        [StatusCodes.Status500InternalServerError] = "Error interno",
    };

    public static string TitleFor(int status) => Titles.GetValueOrDefault(status, "Error");

    public static IMvcBuilder AddSpanishErrors(this IMvcBuilder builder)
    {
        builder.AddMvcOptions(options =>
        {
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            TranslateBindingMessages(options.ModelBindingMessageProvider);
        });
        builder.AddJsonOptions(options => options.AllowInputFormatterExceptionMessages = false);
        builder.ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = CreateValidationResponse);
        builder.Services.Configure<ProblemDetailsOptions>(options => options.CustomizeProblemDetails = TranslateTitle);
        return builder;
    }

    private static void TranslateTitle(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;
        var status = problem.Status ?? context.HttpContext.Response.StatusCode;
        var hasDefaultTitle = problem is HttpValidationProblemDetails
            || problem.Title is null
            || problem.Title == ReasonPhrases.GetReasonPhrase(status);

        if (hasDefaultTitle && Titles.TryGetValue(status, out var title))
        {
            problem.Title = title;
        }
    }

    private static void TranslateBindingMessages(DefaultModelBindingMessageProvider messages)
    {
        messages.SetAttemptedValueIsInvalidAccessor((value, field) => $"El valor '{value}' no es válido para {field}.");
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"El valor '{value}' no es válido.");
        messages.SetUnknownValueIsInvalidAccessor(field => $"El valor indicado no es válido para {field}.");
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => InvalidValue);
        messages.SetValueIsInvalidAccessor(value => $"El valor '{value}' no es válido.");
        messages.SetValueMustBeANumberAccessor(field => $"{field} debe ser un número.");
        messages.SetNonPropertyValueMustBeANumberAccessor(() => "El valor debe ser un número.");
        messages.SetValueMustNotBeNullAccessor(value => $"El valor '{value}' no es válido.");
        messages.SetMissingBindRequiredValueAccessor(field => $"Falta el valor de {field}.");
        messages.SetMissingKeyOrValueAccessor(() => "Falta un valor obligatorio.");
        messages.SetMissingRequestBodyRequiredValueAccessor(() => "El cuerpo de la solicitud es obligatorio.");
    }

    private static IActionResult CreateValidationResponse(ActionContext context)
    {
        var errors = new ModelStateDictionary();
        foreach (var (key, entry) in context.ModelState)
        {
            foreach (var error in entry.Errors)
            {
                var (field, message) = error.Exception is JsonException jsonError
                    ? DescribeJsonError(key, jsonError, context)
                    : (key, string.IsNullOrEmpty(error.ErrorMessage) ? InvalidValue : error.ErrorMessage);
                errors.TryAddModelError(field, message);
            }
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = factory.CreateValidationProblemDetails(context.HttpContext, errors);
        return new BadRequestObjectResult(problem) { ContentTypes = { MediaTypes.ProblemJson } };
    }

    private static (string Field, string Message) DescribeJsonError(string key, JsonException error, ActionContext context)
    {
        var jsonName = key.StartsWith("$.", StringComparison.Ordinal) ? key[2..] : string.Empty;
        var property = BodyProperty(jsonName, context);
        if (error.InnerException is JsonException || property is null)
        {
            return (string.Empty, "El cuerpo de la solicitud no es un JSON válido o no tiene el formato esperado.");
        }

        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var message = type == typeof(long) || type == typeof(int)
            ? $"{jsonName} debe ser un número entero, sin decimales."
            : type == typeof(string)
                ? $"{jsonName} debe ser un texto."
                : $"El valor de {jsonName} no tiene el formato esperado.";
        return (property.Name, message);
    }

    private static PropertyInfo? BodyProperty(string jsonName, ActionContext context) =>
        jsonName.Length == 0
            ? null
            : context.ActionDescriptor.Parameters
                .FirstOrDefault(parameter => parameter.BindingInfo?.BindingSource == BindingSource.Body)
                ?.ParameterType
                .GetProperty(jsonName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
}
