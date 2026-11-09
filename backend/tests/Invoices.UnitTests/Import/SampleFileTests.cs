using Invoices.Application.Import.Rules;
using Invoices.Application.Import.Source;

namespace Invoices.UnitTests.Import;

public sealed class SampleFileTests
{
    private static readonly IInvoiceImportRule[] Rules =
    [
        new TotalsConsistencyRule(),
        new LineSubtotalRule(),
        new DueDateRule(),
        new CustomerRunRule(),
        new DeclaredInvoiceStatusRule(),
        new DeclaredPaymentStatusRule(),
        new CreditNotesWithinTotalRule(),
    ];

    [Fact]
    public async Task El_archivo_de_ejemplo_tiene_las_observaciones_esperadas()
    {
        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "TestData", "bd_exam.json"));
        var read = await InvoiceFileReader.ReadAsync(stream, CancellationToken.None);
        Assert.True(read.IsSuccess);

        var invoices = read.Value
            .Select(record =>
            {
                Assert.True(InvoiceRecordValidator.TryConvert(record, out var invoice, out var errors), string.Join(" ", errors));
                return invoice!;
            })
            .ToList();

        var findings = invoices
            .SelectMany(invoice => Rules.SelectMany(rule => rule.Evaluate(invoice)).Select(finding => (invoice.InvoiceNumber, finding.Code)))
            .ToList();

        int[] InvoicesWith(string code) =>
            findings.Where(item => item.Code == code).Select(item => item.InvoiceNumber).Distinct().Order().ToArray();

        Assert.Equal(50, invoices.Count);
        Assert.Equal(new[] { 41, 48 }, InvoicesWith(ImportIssueCodes.TotalMismatch));
        Assert.Equal(new[] { 41 }, InvoicesWith(ImportIssueCodes.LineSubtotalMismatch));
        Assert.Equal(new[] { 30 }, InvoicesWith(ImportIssueCodes.DeclaredStatusMismatch));
        Assert.Equal(new[] { 1, 3, 6, 8, 23, 25, 30 }, InvoicesWith(ImportIssueCodes.DueDateMismatch));
        Assert.Equal(new[] { 4 }, InvoicesWith(ImportIssueCodes.InvalidRun));
        Assert.Empty(InvoicesWith(ImportIssueCodes.DeclaredPaymentStatusMismatch));
        Assert.Empty(InvoicesWith(ImportIssueCodes.CreditNotesExceedTotal));
        Assert.Empty(InvoicesWith(ImportIssueCodes.UnknownDeclaredStatus));
    }
}
