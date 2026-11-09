using Invoices.Domain.Customers;
using Invoices.Domain.Invoices;
using Invoices.Infrastructure.Persistence.Views;
using Microsoft.EntityFrameworkCore;

namespace Invoices.Infrastructure.Persistence;

public sealed class InvoicesDbContext(DbContextOptions<InvoicesDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();

    public DbSet<InvoiceSummaryView> InvoiceSummaries => Set<InvoiceSummaryView>();

    public DbSet<InconsistentInvoiceView> InconsistentInvoices => Set<InconsistentInvoiceView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoicesDbContext).Assembly);
}
