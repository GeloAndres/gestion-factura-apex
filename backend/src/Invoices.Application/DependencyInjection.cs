using Invoices.Application.Abstractions;
using Invoices.Application.Common;
using Invoices.Application.CreditNotes;
using Invoices.Application.Import;
using Invoices.Application.Import.Rules;
using Invoices.Application.Invoices;
using Invoices.Application.Reports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Invoices.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IBusinessCalendar, BusinessCalendar>();

        services.AddScoped<IInvoiceQueryService, InvoiceQueryService>();
        services.AddScoped<ICreditNoteService, CreditNoteService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IInvoiceImportService, InvoiceImportService>();

        services.AddSingleton<IInvoiceImportRule, TotalsConsistencyRule>();
        services.AddSingleton<IInvoiceImportRule, LineSubtotalRule>();
        services.AddSingleton<IInvoiceImportRule, DueDateRule>();
        services.AddSingleton<IInvoiceImportRule, CustomerRunRule>();
        services.AddSingleton<IInvoiceImportRule, DeclaredInvoiceStatusRule>();
        services.AddSingleton<IInvoiceImportRule, DeclaredPaymentStatusRule>();
        services.AddSingleton<IInvoiceImportRule, CreditNotesWithinTotalRule>();

        return services;
    }
}
