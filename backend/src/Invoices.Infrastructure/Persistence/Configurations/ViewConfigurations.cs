using Invoices.Infrastructure.Persistence.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoices.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceSummaryViewConfiguration : IEntityTypeConfiguration<InvoiceSummaryView>
{
    public void Configure(EntityTypeBuilder<InvoiceSummaryView> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_invoice_summary");
        builder.Property(view => view.InvoiceStatus).HasConversion<string>();
    }
}

internal sealed class InconsistentInvoiceViewConfiguration : IEntityTypeConfiguration<InconsistentInvoiceView>
{
    public void Configure(EntityTypeBuilder<InconsistentInvoiceView> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_inconsistent_invoices");
    }
}
