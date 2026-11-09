using Invoices.Domain.Invoices;

namespace Invoices.Application.Import;

/// <summary>Resumen de una importación.</summary>
public sealed record ImportResultDto(
    string Source,
    int TotalRecords,
    int Imported,
    int ImportedInconsistent,
    IReadOnlyList<int> InconsistentInvoiceNumbers,
    IReadOnlyList<SkippedRecordDto> Skipped,
    IReadOnlyList<ImportIssueReportDto> Issues);

/// <summary>Registro que no se importó (duplicado o incompleto). <c>Position</c> parte en 1.</summary>
public sealed record SkippedRecordDto(int Position, int? InvoiceNumber, string Reason);

public sealed record ImportIssueReportDto(
    int InvoiceNumber,
    string Code,
    ImportIssueSeverity Severity,
    string Message);
