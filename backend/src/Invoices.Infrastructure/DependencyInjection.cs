using Invoices.Application.Abstractions;
using Invoices.Infrastructure.Persistence;
using Invoices.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Invoices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<InvoicesDbContext>((provider, options) =>
        {
            var database = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseSqlite(database.BuildConnectionString());
        });

        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IInvoiceReadRepository, InvoiceReadRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static Task MigrateDatabaseAsync(this IServiceProvider scopedServices, CancellationToken cancellationToken = default) =>
        scopedServices.GetRequiredService<InvoicesDbContext>().Database.MigrateAsync(cancellationToken);
}
