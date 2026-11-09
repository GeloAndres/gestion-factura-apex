using Invoices.Domain.Common;
using Invoices.Domain.Customers;

namespace Invoices.Domain.Invoices;

public sealed class Invoice
{
    private readonly List<InvoiceLine> _lines = [];
    private readonly List<CreditNote> _creditNotes = [];
    private readonly List<ImportIssue> _importIssues = [];

    private Invoice()
    {
    }

    public Invoice(
        int invoiceNumber,
        DateOnly invoiceDate,
        int daysToDue,
        DateOnly paymentDueDate,
        long totalAmount,
        Customer customer)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(invoiceNumber);
        ArgumentOutOfRangeException.ThrowIfNegative(totalAmount);
        ArgumentNullException.ThrowIfNull(customer);

        InvoiceNumber = invoiceNumber;
        InvoiceDate = invoiceDate;
        DaysToDue = daysToDue;
        PaymentDueDate = paymentDueDate;
        TotalAmount = totalAmount;
        Customer = customer;
    }

    public int Id { get; private set; }

    public int InvoiceNumber { get; private set; }

    public DateOnly InvoiceDate { get; private set; }

    public int DaysToDue { get; private set; }

    public DateOnly PaymentDueDate { get; private set; }

    public long TotalAmount { get; private set; }

    public bool IsConsistent { get; private set; } = true;

    public int CustomerId { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public Payment? Payment { get; private set; }

    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    public IReadOnlyCollection<CreditNote> CreditNotes => _creditNotes.AsReadOnly();

    public IReadOnlyCollection<ImportIssue> ImportIssues => _importIssues.AsReadOnly();

    public long LinesTotal => _lines.Sum(line => line.Subtotal);

    public long CreditNotesTotal => _creditNotes.Sum(note => note.Amount);

    public long PendingBalance => Math.Max(0, TotalAmount - CreditNotesTotal);

    public InvoiceStatus Status => InvoiceStatusRules.Resolve(TotalAmount, CreditNotesTotal);

    public PaymentStatus GetPaymentStatus(DateOnly today) =>
        PaymentStatusRules.Resolve(Payment?.PaymentDate, PaymentDueDate, today);

    public void AddLine(string productName, long unitPrice, int quantity, long subtotal) =>
        _lines.Add(new InvoiceLine(productName, unitPrice, quantity, subtotal));

    public void RegisterImportedCreditNote(int number, DateOnly date, long amount) =>
        _creditNotes.Add(new CreditNote(number, date, amount));

    public void RegisterPayment(string? method, DateOnly paymentDate) =>
        Payment = new Payment(method, paymentDate);

    public void RecordImportIssue(string code, ImportIssueSeverity severity, string message)
    {
        _importIssues.Add(new ImportIssue(code, severity, message));
        if (severity == ImportIssueSeverity.Inconsistency)
        {
            IsConsistent = false;
        }
    }

    public Result<CreditNote> AddCreditNote(int creditNoteNumber, long amount, DateOnly date)
    {
        if (!IsConsistent)
        {
            return InvoiceErrors.Inconsistent(InvoiceNumber);
        }

        if (amount <= 0)
        {
            return InvoiceErrors.InvalidCreditNoteAmount;
        }

        var pendingBalance = PendingBalance;
        if (pendingBalance == 0)
        {
            return InvoiceErrors.NoPendingBalance(InvoiceNumber);
        }

        if (amount > pendingBalance)
        {
            return InvoiceErrors.CreditNoteExceedsPendingBalance(amount, pendingBalance);
        }

        var creditNote = new CreditNote(creditNoteNumber, date, amount);
        _creditNotes.Add(creditNote);
        return creditNote;
    }
}
