CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "Customers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Customers" PRIMARY KEY AUTOINCREMENT,
    "Run" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Email" TEXT NULL
);

CREATE TABLE "Invoices" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Invoices" PRIMARY KEY AUTOINCREMENT,
    "InvoiceNumber" INTEGER NOT NULL,
    "InvoiceDate" TEXT NOT NULL,
    "DaysToDue" INTEGER NOT NULL,
    "PaymentDueDate" TEXT NOT NULL,
    "TotalAmount" INTEGER NOT NULL,
    "IsConsistent" INTEGER NOT NULL,
    "CustomerId" INTEGER NOT NULL,
    CONSTRAINT "FK_Invoices_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CreditNotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CreditNotes" PRIMARY KEY AUTOINCREMENT,
    "InvoiceId" INTEGER NOT NULL,
    "Number" INTEGER NOT NULL,
    "Date" TEXT NOT NULL,
    "Amount" INTEGER NOT NULL,
    CONSTRAINT "FK_CreditNotes_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE
);

CREATE TABLE "InvoiceImportIssues" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InvoiceImportIssues" PRIMARY KEY AUTOINCREMENT,
    "InvoiceId" INTEGER NOT NULL,
    "Code" TEXT NOT NULL,
    "Severity" TEXT NOT NULL,
    "Message" TEXT NOT NULL,
    CONSTRAINT "FK_InvoiceImportIssues_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE
);

CREATE TABLE "InvoiceLines" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_InvoiceLines" PRIMARY KEY AUTOINCREMENT,
    "InvoiceId" INTEGER NOT NULL,
    "ProductName" TEXT NOT NULL,
    "UnitPrice" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "Subtotal" INTEGER NOT NULL,
    CONSTRAINT "FK_InvoiceLines_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Payments" (
    "InvoiceId" INTEGER NOT NULL CONSTRAINT "PK_Payments" PRIMARY KEY,
    "Method" TEXT NULL,
    "PaymentDate" TEXT NOT NULL,
    CONSTRAINT "FK_Payments_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_CreditNotes_InvoiceId" ON "CreditNotes" ("InvoiceId");

CREATE UNIQUE INDEX "IX_CreditNotes_Number" ON "CreditNotes" ("Number");

CREATE UNIQUE INDEX "IX_Customers_Run" ON "Customers" ("Run");

CREATE INDEX "IX_InvoiceImportIssues_InvoiceId" ON "InvoiceImportIssues" ("InvoiceId");

CREATE INDEX "IX_InvoiceLines_InvoiceId" ON "InvoiceLines" ("InvoiceId");

CREATE INDEX "IX_Invoices_CustomerId" ON "Invoices" ("CustomerId");

CREATE UNIQUE INDEX "IX_Invoices_InvoiceNumber" ON "Invoices" ("InvoiceNumber");

CREATE INDEX "IX_Invoices_IsConsistent" ON "Invoices" ("IsConsistent");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261107195746_InitialCreate', '8.0.33');

COMMIT;

BEGIN TRANSACTION;

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

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261107195749_AddReportingViews', '8.0.33');

COMMIT;

