namespace Invoices.Application.Import.Source;

public static class InvoiceRecordValidator
{
    public static bool TryConvert(InvoiceRecord? record, out ImportedInvoice? invoice, out IReadOnlyList<string> errors)
    {
        invoice = null;
        if (record is null)
        {
            errors = ["El registro está vacío."];
            return false;
        }

        var problems = new List<string>();

        if (record.InvoiceNumber is not > 0)
        {
            problems.Add("invoice_number es obligatorio y debe ser mayor a 0.");
        }

        if (record.InvoiceDate is null)
        {
            problems.Add("invoice_date es obligatorio.");
        }

        if (record.TotalAmount is null or < 0)
        {
            problems.Add("total_amount es obligatorio y no puede ser negativo.");
        }

        if (record.PaymentDueDate is null)
        {
            problems.Add("payment_due_date es obligatorio.");
        }

        if (record.DaysToDue is < 0)
        {
            problems.Add("days_to_due no puede ser negativo.");
        }

        var lines = ConvertLines(record.InvoiceDetail, problems);
        var creditNotes = ConvertCreditNotes(record.InvoiceCreditNote, problems);
        var customer = ConvertCustomer(record.Customer, problems);

        if (problems.Count > 0 || customer is null)
        {
            errors = problems;
            return false;
        }

        var invoiceDate = record.InvoiceDate!.Value;
        var paymentDueDate = record.PaymentDueDate!.Value;
        var payment = record.InvoicePayment?.PaymentDate is { } paymentDate
            ? new ImportedPayment(record.InvoicePayment.PaymentMethod, paymentDate)
            : null;

        invoice = new ImportedInvoice(
            record.InvoiceNumber!.Value,
            invoiceDate,
            record.InvoiceStatus,
            record.TotalAmount!.Value,
            record.DaysToDue ?? (paymentDueDate.DayNumber - invoiceDate.DayNumber),
            paymentDueDate,
            record.PaymentStatus,
            lines,
            payment,
            creditNotes,
            customer);
        errors = [];
        return true;
    }

    private static List<ImportedLine> ConvertLines(List<InvoiceDetailRecord?>? records, List<string> problems)
    {
        var lines = new List<ImportedLine>();
        var position = 0;
        foreach (var line in records ?? [])
        {
            position++;
            if (line is null
                || string.IsNullOrWhiteSpace(line.ProductName)
                || line.UnitPrice is null or < 0
                || line.Quantity is null or < 0
                || line.Subtotal is null or < 0)
            {
                problems.Add($"invoice_detail[{position}] requiere product_name, unit_price, quantity y subtotal no negativos.");
                continue;
            }

            lines.Add(new ImportedLine(line.ProductName.Trim(), line.UnitPrice.Value, line.Quantity.Value, line.Subtotal.Value));
        }

        return lines;
    }

    private static List<ImportedCreditNote> ConvertCreditNotes(List<CreditNoteRecord?>? records, List<string> problems)
    {
        var creditNotes = new List<ImportedCreditNote>();
        var position = 0;
        foreach (var note in records ?? [])
        {
            position++;
            if (note is null
                || note.CreditNoteNumber is not > 0
                || note.CreditNoteDate is null
                || note.CreditNoteAmount is not > 0)
            {
                problems.Add($"invoice_credit_note[{position}] requiere credit_note_number, credit_note_date y credit_note_amount mayor a 0.");
                continue;
            }

            creditNotes.Add(new ImportedCreditNote(
                note.CreditNoteNumber.Value,
                note.CreditNoteDate.Value,
                note.CreditNoteAmount.Value));
        }

        return creditNotes;
    }

    private static ImportedCustomer? ConvertCustomer(CustomerRecord? record, List<string> problems)
    {
        if (record is null
            || string.IsNullOrWhiteSpace(record.CustomerRun)
            || string.IsNullOrWhiteSpace(record.CustomerName))
        {
            problems.Add("customer requiere customer_run y customer_name.");
            return null;
        }

        return new ImportedCustomer(record.CustomerRun.Trim(), record.CustomerName.Trim(), record.CustomerEmail?.Trim());
    }
}
