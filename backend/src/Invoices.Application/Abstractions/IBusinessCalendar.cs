namespace Invoices.Application.Abstractions;

/// <summary>Entrega la fecha de "hoy" del negocio (zona horaria de Chile, no UTC).</summary>
public interface IBusinessCalendar
{
    DateOnly Today { get; }
}
