using Invoices.Application.Import.Source;
using Invoices.Domain.Invoices;

namespace Invoices.Application.Import.Rules;

public interface IInvoiceImportRule
{
    IEnumerable<ImportFinding> Evaluate(ImportedInvoice invoice);
}

public sealed record ImportFinding(string Code, ImportIssueSeverity Severity, string Message)
{
    public static ImportFinding Warning(string code, string message) =>
        new(code, ImportIssueSeverity.Warning, message);

    public static ImportFinding Inconsistency(string code, string message) =>
        new(code, ImportIssueSeverity.Inconsistency, message);
}

public static class ImportIssueCodes
{
    public const string TotalMismatch = "TOTAL_MISMATCH";
    public const string LineSubtotalMismatch = "LINE_SUBTOTAL_MISMATCH";
    public const string DueDateMismatch = "DUE_DATE_MISMATCH";
    public const string InvalidRun = "INVALID_RUN";
    public const string DeclaredStatusMismatch = "DECLARED_STATUS_MISMATCH";
    public const string UnknownDeclaredStatus = "UNKNOWN_DECLARED_STATUS";
    public const string DeclaredPaymentStatusMismatch = "DECLARED_PAYMENT_STATUS_MISMATCH";
    public const string CreditNotesExceedTotal = "CREDIT_NOTES_EXCEED_TOTAL";
}
