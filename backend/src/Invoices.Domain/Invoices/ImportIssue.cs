namespace Invoices.Domain.Invoices;

public enum ImportIssueSeverity
{
    Warning,
    Inconsistency,
}

public sealed class ImportIssue
{
    private ImportIssue()
    {
    }

    internal ImportIssue(string code, ImportIssueSeverity severity, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Code = code;
        Severity = severity;
        Message = message;
    }

    public int Id { get; private set; }

    public int InvoiceId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public ImportIssueSeverity Severity { get; private set; }

    public string Message { get; private set; } = string.Empty;
}
