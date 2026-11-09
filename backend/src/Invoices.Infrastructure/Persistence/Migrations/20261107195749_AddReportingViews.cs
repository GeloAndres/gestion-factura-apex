using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Invoices.Infrastructure.Persistence.Migrations
{
    public partial class AddReportingViews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE VIEW vw_invoice_summary AS
                SELECT
                    i.Id                                                       AS InvoiceId,
                    i.InvoiceNumber                                            AS InvoiceNumber,
                    i.InvoiceDate                                              AS InvoiceDate,
                    i.DaysToDue                                                AS DaysToDue,
                    i.PaymentDueDate                                           AS PaymentDueDate,
                    i.TotalAmount                                              AS TotalAmount,
                    c.Run                                                      AS CustomerRun,
                    c.Name                                                     AS CustomerName,
                    COALESCE(cn.CreditNotesCount, 0)                           AS CreditNotesCount,
                    COALESCE(cn.CreditNotesTotal, 0)                           AS CreditNotesTotal,
                    MAX(i.TotalAmount - COALESCE(cn.CreditNotesTotal, 0), 0)   AS PendingBalance,
                    CASE
                        WHEN COALESCE(cn.CreditNotesTotal, 0) <= 0     THEN 'Issued'
                        WHEN cn.CreditNotesTotal >= i.TotalAmount      THEN 'Cancelled'
                        ELSE 'Partial'
                    END                                                        AS InvoiceStatus,
                    p.PaymentDate                                              AS PaymentDate,
                    p.Method                                                   AS PaymentMethod
                FROM Invoices i
                JOIN Customers c ON c.Id = i.CustomerId
                LEFT JOIN (
                    SELECT InvoiceId, COUNT(*) AS CreditNotesCount, SUM(Amount) AS CreditNotesTotal
                    FROM CreditNotes
                    GROUP BY InvoiceId
                ) cn ON cn.InvoiceId = i.Id
                LEFT JOIN Payments p ON p.InvoiceId = i.Id
                WHERE i.IsConsistent = 1;
                """);

            migrationBuilder.Sql("""
                CREATE VIEW vw_inconsistent_invoices AS
                SELECT
                    i.Id                                          AS InvoiceId,
                    i.InvoiceNumber                               AS InvoiceNumber,
                    i.InvoiceDate                                 AS InvoiceDate,
                    c.Run                                         AS CustomerRun,
                    c.Name                                        AS CustomerName,
                    i.TotalAmount                                 AS DeclaredTotal,
                    COALESCE(l.LinesTotal, 0)                     AS LinesTotal,
                    i.TotalAmount - COALESCE(l.LinesTotal, 0)     AS Difference
                FROM Invoices i
                JOIN Customers c ON c.Id = i.CustomerId
                LEFT JOIN (
                    SELECT InvoiceId, SUM(Subtotal) AS LinesTotal
                    FROM InvoiceLines
                    GROUP BY InvoiceId
                ) l ON l.InvoiceId = i.Id
                WHERE i.IsConsistent = 0;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_inconsistent_invoices;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_invoice_summary;");
        }
    }
}
