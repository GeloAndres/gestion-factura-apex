namespace Invoices.Domain.Common;

/// <summary>Tipo de error de negocio. La API lo traduce a un código HTTP.</summary>
public enum ErrorType
{
    /// <summary>Datos de entrada inválidos (HTTP 400).</summary>
    Validation,

    /// <summary>El recurso no existe en el sistema activo (HTTP 404).</summary>
    NotFound,

    /// <summary>Conflicto con el estado actual de los datos (HTTP 409).</summary>
    Conflict,

    /// <summary>La operación viola una regla de negocio (HTTP 422).</summary>
    BusinessRule,
}

/// <summary>Error esperado de una operación (no es una excepción).</summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error BusinessRule(string code, string message) => new(code, message, ErrorType.BusinessRule);
}
