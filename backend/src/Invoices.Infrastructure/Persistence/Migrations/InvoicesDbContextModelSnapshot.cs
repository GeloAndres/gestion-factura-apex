using System;
using Invoices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Invoices.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(InvoicesDbContext))]
    partial class InvoicesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.33");

            modelBuilder.Entity("Invoices.Domain.Customers.Customer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .HasMaxLength(320)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("Run")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Run")
                        .IsUnique();

                    b.ToTable("Customers", (string)null);
                });

            modelBuilder.Entity("Invoices.Domain.Invoices.CreditNote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("Amount")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("TEXT");

                    b.Property<int>("InvoiceId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceId");

                    b.HasIndex("Number")
                        .IsUnique();

                    b.ToTable("CreditNotes", (string)null);
                });

            modelBuilder.Entity("Invoices.Domain.Invoices.ImportIssue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("InvoiceId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<string>("Severity")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceId");

                    b.ToTable("InvoiceImportIssues", (string)null);
                });

            modelBuilder.Entity("Invoices.Domain.Invoices.Invoice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CustomerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DaysToDue")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("InvoiceDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("InvoiceNumber")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsConsistent")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("PaymentDueDate")
                        .HasColumnType("TEXT");

                    b.Property<long>("TotalAmount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("InvoiceNumber")
                        .IsUnique();

                    b.HasIndex("IsConsistent");

                    b.ToTable("Invoices", (string)null);
                });

            modelBuilder.Entity("Invoices.Domain.Invoices.InvoiceLine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("InvoiceId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<int>("Quantity")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Subtotal")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UnitPrice")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceId");

                    b.ToTable("InvoiceLines", (string)null);
                });

            modelBuilder.Entity("Invoices.Infrastructure.Persistence.Views.InconsistentInvoiceView", b =>
                {
                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CustomerRun")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("DeclaredTotal")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Difference")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("InvoiceDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("InvoiceId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("InvoiceNumber")
                        .HasColumnType("INTEGER");

                    b.Property<long>("LinesTotal")
                        .HasColumnType("INTEGER");

                    b.ToTable((string)null);

                    b.ToView("vw_inconsistent_invoices", (string)null);
                });

            modelBuilder.Entity("Invoices.Infrastructure.Persistence.Views.InvoiceSummaryView", b =>
                {
                    b.Property<int>("CreditNotesCount")
                        .HasColumnType("INTEGER");

                    b.Property<long>("CreditNotesTotal")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CustomerRun")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("DaysToDue")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("InvoiceDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("InvoiceId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("InvoiceNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("InvoiceStatus")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateOnly?>("PaymentDate")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("PaymentDueDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("PaymentMethod")
                        .HasColumnType("TEXT");

                    b.Property<long>("PendingBalance")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TotalAmount")
                        .HasColumnType("INTEGER");

                    b.ToTable((string)null);

                    b.ToView("vw_invoice_summary", (string)null);
                });

            modelBuilder.Entity("Invoices.Domain.Invoices.CreditNote", b =>
                {
                    b.HasOne("Invoices.Domain.Invoices.Invoice", null)
                        .WithMany("CreditNotes")
                        .HasForeignKey("InvoiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Invoices.Domain.Invoices.ImportIssue", b =>
                {
                    b.HasOne("Invoices.Domain.Invoices.Invoice", null)
                        .WithMany("ImportIssues")
                        .HasForeignKey("InvoiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Invoices.Domain.Invoices.Invoice", b =>
                {
                    b.HasOne("Invoices.Domain.Customers.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.OwnsOne("Invoices.Domain.Invoices.Payment", "Payment", b1 =>
                        {
                            b1.Property<int>("InvoiceId")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Method")
                                .HasMaxLength(50)
                                .HasColumnType("TEXT");

                            b1.Property<DateOnly>("PaymentDate")
                                .HasColumnType("TEXT");

                            b1.HasKey("InvoiceId");

                            b1.ToTable("Payments", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("InvoiceId");
                        });

                    b.Navigation("Customer");

                    b.Navigation("Payment");
                });

            modelBuilder.Entity("Invoices.Domain.Invoices.InvoiceLine", b =>
                {
                    b.HasOne("Invoices.Domain.Invoices.Invoice", null)
                        .WithMany("Lines")
                        .HasForeignKey("InvoiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Invoices.Domain.Invoices.Invoice", b =>
                {
                    b.Navigation("CreditNotes");

                    b.Navigation("ImportIssues");

                    b.Navigation("Lines");
                });
#pragma warning restore 612, 618
        }
    }
}
