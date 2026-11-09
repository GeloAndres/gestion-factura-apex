using System.ComponentModel.DataAnnotations;
using Invoices.Application.Invoices;
using Invoices.Domain.Invoices;

namespace Invoices.Api.Contracts;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string? Username { get; init; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string? Password { get; init; }
}

public sealed record LoginResponse(string AccessToken, string TokenType, DateTimeOffset ExpiresAt, string Username);

public sealed class InvoiceSearchQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "El número de factura debe ser un entero positivo.")]
    public int? Number { get; init; }

    public InvoiceStatus? InvoiceStatus { get; init; }

    public PaymentStatus? PaymentStatus { get; init; }

    public DateOnly? AsOf { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor o igual a 1.")]
    public int Page { get; init; } = 1;

    [Range(1, InvoiceQueryService.MaxPageSize, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int PageSize { get; init; } = 20;

    public InvoiceSearchCriteria ToCriteria() => new(Number, InvoiceStatus, PaymentStatus, Page, PageSize);
}

public sealed class CreateCreditNoteRequest
{
    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(1, long.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
    public long? Amount { get; init; }
}

public sealed class ImportFileRequest
{
    [Required(ErrorMessage = "Selecciona un archivo .json.")]
    public IFormFile? File { get; init; }
}
