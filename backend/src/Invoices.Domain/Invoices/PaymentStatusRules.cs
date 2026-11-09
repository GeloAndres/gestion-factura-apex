namespace Invoices.Domain.Invoices;

public static class PaymentStatusRules
{
    public static PaymentStatus Resolve(DateOnly? paymentDate, DateOnly paymentDueDate, DateOnly today) =>
        paymentDate is not null ? PaymentStatus.Paid
        : paymentDueDate < today ? PaymentStatus.Overdue
        : PaymentStatus.Pending;

    public static int DaysOverdue(DateOnly paymentDueDate, DateOnly today) =>
        Math.Max(0, today.DayNumber - paymentDueDate.DayNumber);
}
