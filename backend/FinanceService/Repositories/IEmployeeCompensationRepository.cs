using FinanceService.Models;

namespace FinanceService.Repositories
{
    public interface IEmployeeCompensationRepository
    {
        Task<EmployeeCompensationAccount?> GetByEmployeeIdAsync(string employeeId, bool includeAdjustments = false, CancellationToken cancellationToken = default);
        Task<bool> EmployeeAccountExistsAsync(string employeeId, CancellationToken cancellationToken = default);
        Task AddAccountAsync(EmployeeCompensationAccount account, CancellationToken cancellationToken = default);
        void UpdateAccount(EmployeeCompensationAccount account);
        Task AddBonusesAsync(IEnumerable<EmployeeBonus> bonuses, CancellationToken cancellationToken = default);
        Task AddDeductionsAsync(IEnumerable<EmployeeDeduction> deductions, CancellationToken cancellationToken = default);
        Task<(decimal bonuses, decimal deductions)> GetYearToDateTotalsAsync(int accountId, int year, CancellationToken cancellationToken = default);
        Task<IEnumerable<EmployeeBonus>> GetRecentBonusesAsync(int accountId, int take, CancellationToken cancellationToken = default);
        Task<IEnumerable<EmployeeDeduction>> GetRecentDeductionsAsync(int accountId, int take, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
