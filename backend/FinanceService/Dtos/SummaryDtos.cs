namespace FinanceService.Dtos
{
    public record FinanceSummaryDto(
        decimal OutstandingReceivables,
        decimal OverdueReceivables,
        decimal CurrentMonthExpenseTotal,
        decimal BudgetUtilization,
        decimal UpcomingPayrollCommitment);
}
