namespace Invoices.Application.Abstractions;

public interface IBusinessCalendar
{
    DateOnly Today { get; }
}
