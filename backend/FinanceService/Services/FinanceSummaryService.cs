using FinanceService.Data;
using FinanceService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Services
{
    public class FinanceSummaryService : IFinanceSummaryService
    {
        private readonly AppDbContext _context;

        public FinanceSummaryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FinanceSummaryDto> BuildSummaryAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            var outstandingReceivables = await _context.Invoices
                .Where(invoice => invoice.TotalAmount > invoice.PaidAmount)
                .Select(invoice => invoice.TotalAmount - invoice.PaidAmount)
                .SumAsync(cancellationToken);

            var overdueReceivables = await _context.Invoices
                .Where(invoice => invoice.TotalAmount > invoice.PaidAmount &&
                                  invoice.DueDate < now)
                .Select(invoice => invoice.TotalAmount - invoice.PaidAmount)
                .SumAsync(cancellationToken);

            var currentMonthExpenseTotal = await _context.Expenses
                .Where(expense => expense.IncurredOn >= startOfMonth && expense.IncurredOn <= endOfMonth)
                .Select(expense => expense.Amount)
                .SumAsync(cancellationToken);

            var budgetTotals = await _context.Budgets
                .Select(budget => new { budget.TotalAmount, budget.SpentAmount })
                .ToListAsync(cancellationToken);

            decimal budgetUtilization = 0m;
            if (budgetTotals.Count > 0)
            {
                var total = budgetTotals.Sum(x => x.TotalAmount);
                var spent = budgetTotals.Sum(x => x.SpentAmount);
                budgetUtilization = total == 0 ? 0 : Math.Round(spent / total, 4);
            }

            var upcomingPayrollCommitment = await _context.PayrollRuns
                .Where(run => run.PaymentDate >= now)
                .Select(run => run.TotalNetPay)
                .SumAsync(cancellationToken);

            return new FinanceSummaryDto(
                DecimalRound(outstandingReceivables),
                DecimalRound(overdueReceivables),
                DecimalRound(currentMonthExpenseTotal),
                DecimalRound(budgetUtilization),
                DecimalRound(upcomingPayrollCommitment));
        }

        private static decimal DecimalRound(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
