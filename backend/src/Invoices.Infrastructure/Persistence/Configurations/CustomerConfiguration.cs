using Invoices.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoices.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Run).HasMaxLength(20).IsRequired();
        builder.HasIndex(customer => customer.Run).IsUnique();

        builder.Property(customer => customer.Name).HasMaxLength(200).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(320);
    }
}
