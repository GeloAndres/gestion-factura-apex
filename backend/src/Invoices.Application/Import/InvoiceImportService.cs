using Invoices.Application.Abstractions;
using Invoices.Application.Import.Rules;
using Invoices.Application.Import.Source;
using Invoices.Domain.Common;
using Invoices.Domain.Customers;
using Invoices.Domain.Invoices;
using Microsoft.Extensions.Logging;

namespace Invoices.Application.Import;

public interface IInvoiceImportService
{
    Task<Result<ImportResultDto>> ImportAsync(Stream content, string sourceName, CancellationToken cancellationToken);
}

/// <summary>
/// Importa facturas desde un JSON. Es idempotente: las facturas cuyo invoice_number ya existe se omiten,
/// así que el mismo archivo se puede cargar varias veces sin duplicar datos.
/// </summary>
public sealed class InvoiceImportService(
    IInvoiceRepository invoices,
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    IEnumerable<IInvoiceImportRule> rules,
    ILogger<InvoiceImportService> logger) : IInvoiceImportService
{
    private readonly IReadOnlyList<IInvoiceImportRule> _rules = rules.ToList();

    public async Task<Result<ImportResultDto>> ImportAsync(
        Stream content,
        string sourceName,
        CancellationToken cancellationToken)
    {
        var read = await InvoiceFileReader.ReadAsync(content, cancellationToken);
        if (read.IsFailure)
        {
            return read.Error;
        }

        var records = read.Value;
        var skipped = new List<SkippedRecordDto>();
        var candidates = new List<(int Position, ImportedInvoice Invoice)>();

        for (var index = 0; index < records.Count; index++)
        {
            if (InvoiceRecordValidator.TryConvert(records[index], out var invoice, out var errors))
            {
                candidates.Add((index + 1, invoice!));
            }
            else
            {
                skipped.Add(new SkippedRecordDto(index + 1, records[index]?.InvoiceNumber, string.Join(" ", errors)));
            }
        }

        return await unitOfWork.ExecuteInTransactionAsync<ImportResultDto>(
            async ct =>
            {
                var accepted = await RemoveDuplicatesAsync(candidates, skipped, ct);
                var (inconsistent, issues) = await AddInvoicesAsync(accepted, ct);
                await unitOfWork.SaveChangesAsync(ct);

                logger.LogInformation(
                    "Importación de {Source}: {Total} registros, {Imported} importadas ({Inconsistent} inconsistentes), {Skipped} omitidas",
                    sourceName,
                    records.Count,
                    accepted.Count,
                    inconsistent.Count,
                    skipped.Count);

                return new ImportResultDto(
                    sourceName,
                    records.Count,
                    accepted.Count,
                    inconsistent.Count,
                    inconsistent,
                    skipped.OrderBy(item => item.Position).ToList(),
                    issues);
            },
            cancellationToken);
    }

    /// <summary>Descarta facturas ya existentes o repetidas en el archivo, y notas de crédito con número repetido.</summary>
    private async Task<List<ImportedInvoice>> RemoveDuplicatesAsync(
        IReadOnlyList<(int Position, ImportedInvoice Invoice)> candidates,
        List<SkippedRecordDto> skipped,
        CancellationToken cancellationToken)
    {
        var existingInvoices = await invoices.GetExistingInvoiceNumbersAsync(
            candidates.Select(candidate => candidate.Invoice.InvoiceNumber).Distinct().ToList(),
            cancellationToken);
        var existingCreditNotes = await invoices.GetExistingCreditNoteNumbersAsync(
            candidates.SelectMany(candidate => candidate.Invoice.CreditNotes).Select(note => note.Number).Distinct().ToList(),
            cancellationToken);

        var seenInvoices = new HashSet<int>();
        var seenCreditNotes = new HashSet<int>();
        var accepted = new List<ImportedInvoice>();

        foreach (var (position, invoice) in candidates)
        {
            if (existingInvoices.Contains(invoice.InvoiceNumber))
            {
                skipped.Add(new SkippedRecordDto(position, invoice.InvoiceNumber, "Ya existe una factura con este invoice_number."));
                continue;
            }

            if (seenInvoices.Contains(invoice.InvoiceNumber))
            {
                skipped.Add(new SkippedRecordDto(position, invoice.InvoiceNumber, "invoice_number repetido en el archivo; se conserva la primera aparición."));
                continue;
            }

            var creditNoteNumbers = invoice.CreditNotes.Select(note => note.Number).ToList();
            var repeated = creditNoteNumbers
                .Where(number => existingCreditNotes.Contains(number) || seenCreditNotes.Contains(number))
                .Concat(creditNoteNumbers.GroupBy(number => number).Where(group => group.Count() > 1).Select(group => group.Key))
                .Distinct()
                .ToList();
            if (repeated.Count > 0)
            {
                skipped.Add(new SkippedRecordDto(
                    position,
                    invoice.InvoiceNumber,
                    $"Número(s) de nota de crédito repetido(s): {string.Join(", ", repeated)}."));
                continue;
            }

            seenInvoices.Add(invoice.InvoiceNumber);
            seenCreditNotes.UnionWith(creditNoteNumbers);
            accepted.Add(invoice);
        }

        return accepted;
    }

    private async Task<(List<int> Inconsistent, List<ImportIssueReportDto> Issues)> AddInvoicesAsync(
        IReadOnlyList<ImportedInvoice> accepted,
        CancellationToken cancellationToken)
    {
        var runs = accepted.Select(invoice => ChileanRun.Normalize(invoice.Customer.Run)).Distinct().ToList();
        var customerByRun = new Dictionary<string, Customer>(await customers.GetByRunsAsync(runs, cancellationToken));

        var inconsistent = new List<int>();
        var issues = new List<ImportIssueReportDto>();

        foreach (var imported in accepted)
        {
            var run = ChileanRun.Normalize(imported.Customer.Run);
            if (!customerByRun.TryGetValue(run, out var customer))
            {
                customer = new Customer(imported.Customer.Run, imported.Customer.Name, imported.Customer.Email);
                customers.Add(customer);
                customerByRun[run] = customer;
            }

            var invoice = new Invoice(
                imported.InvoiceNumber,
                imported.InvoiceDate,
                imported.DaysToDue,
                imported.PaymentDueDate,
                imported.TotalAmount,
                customer);

            foreach (var line in imported.Lines)
            {
                invoice.AddLine(line.ProductName, line.UnitPrice, line.Quantity, line.Subtotal);
            }

            foreach (var note in imported.CreditNotes)
            {
                invoice.RegisterImportedCreditNote(note.Number, note.Date, note.Amount);
            }

            if (imported.Payment is { } payment)
            {
                invoice.RegisterPayment(payment.Method, payment.PaymentDate);
            }

            foreach (var finding in _rules.SelectMany(rule => rule.Evaluate(imported)))
            {
                invoice.RecordImportIssue(finding.Code, finding.Severity, finding.Message);
                issues.Add(new ImportIssueReportDto(imported.InvoiceNumber, finding.Code, finding.Severity, finding.Message));
            }

            if (!invoice.IsConsistent)
            {
                inconsistent.Add(invoice.InvoiceNumber);
            }

            invoices.Add(invoice);
        }

        return (inconsistent, issues);
    }
}
