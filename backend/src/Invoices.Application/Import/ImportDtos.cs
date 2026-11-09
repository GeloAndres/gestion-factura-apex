using Invoices.Domain.Invoices;

namespace Invoices.Application.Import;

public sealed record ImportResultDto(
    string Source,
    int TotalRecords,
    int Imported,
    int ImportedInconsistent,
    IReadOnlyList<int> InconsistentInvoiceNumbers,
    IReadOnlyList<SkippedRecordDto> Skipped,
    IReadOnlyList<ImportIssueReportDto> Issues);

public sealed record SkippedRecordDto(int Position, int? InvoiceNumber, string Reason);

public sealed record ImportIssueReportDto(
    int InvoiceNumber,
    string Code,
    ImportIssueSeverity Severity,
    string Message);
