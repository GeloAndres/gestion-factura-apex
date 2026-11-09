using Invoices.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoices.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(invoice => invoice.Id);

        builder.HasIndex(invoice => invoice.InvoiceNumber).IsUnique();
        builder.HasIndex(invoice => invoice.IsConsistent);

        builder.HasOne(invoice => invoice.Customer)
            .WithMany()
            .HasForeignKey(invoice => invoice.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(invoice => invoice.Lines)
            .WithOne()
            .HasForeignKey(line => line.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(invoice => invoice.CreditNotes)
            .WithOne()
            .HasForeignKey(note => note.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(invoice => invoice.ImportIssues)
            .WithOne()
            .HasForeignKey(issue => issue.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(invoice => invoice.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(invoice => invoice.CreditNotes).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(invoice => invoice.ImportIssues).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne(invoice => invoice.Payment, payment =>
        {
            payment.ToTable("Payments");
            payment.WithOwner().HasForeignKey("InvoiceId");
            payment.Property(p => p.Method).HasMaxLength(50);
            payment.Property(p => p.PaymentDate).IsRequired();
        });
    }
}
