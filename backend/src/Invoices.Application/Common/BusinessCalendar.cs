using System.ComponentModel.DataAnnotations;
using Invoices.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Invoices.Application.Common;

public sealed class BusinessCalendarOptions
{
    public const string SectionName = "BusinessCalendar";

    /// <summary>Zona horaria IANA con la que se determina "hoy".</summary>
    [Required]
    public string TimeZone { get; set; } = "America/Santiago";
}

/// <summary>
/// "Hoy" según la zona horaria del negocio. Usa <see cref="TimeProvider"/> para que los tests
/// puedan fijar la fecha.
/// </summary>
public sealed class BusinessCalendar : IBusinessCalendar
{
    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _timeZone;

    public BusinessCalendar(TimeProvider timeProvider, IOptions<BusinessCalendarOptions> options)
    {
        _timeProvider = timeProvider;
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);
    }

    public DateOnly Today =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), _timeZone).DateTime);
}
