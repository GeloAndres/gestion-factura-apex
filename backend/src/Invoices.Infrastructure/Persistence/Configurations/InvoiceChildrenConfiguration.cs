using Invoices.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoices.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");
        builder.HasKey(line => line.Id);
        builder.Property(line => line.ProductName).HasMaxLength(200).IsRequired();
    }
}

internal sealed class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.ToTable("CreditNotes");
        builder.HasKey(note => note.Id);
        builder.HasIndex(note => note.Number).IsUnique();
    }
}

internal sealed class ImportIssueConfiguration : IEntityTypeConfiguration<ImportIssue>
{
    public void Configure(EntityTypeBuilder<ImportIssue> builder)
    {
        builder.ToTable("InvoiceImportIssues");
        builder.HasKey(issue => issue.Id);
        builder.Property(issue => issue.Code).HasMaxLength(50).IsRequired();
        builder.Property(issue => issue.Severity).HasConversion<string>().HasMaxLength(20);
        builder.Property(issue => issue.Message).HasMaxLength(500).IsRequired();
    }
}
