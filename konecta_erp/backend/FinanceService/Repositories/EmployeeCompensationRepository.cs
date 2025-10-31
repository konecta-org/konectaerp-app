using FinanceService.Data;
using FinanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Repositories
{
    public class EmployeeCompensationRepository : IEmployeeCompensationRepository
    {
        private readonly AppDbContext _context;

        public EmployeeCompensationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeCompensationAccount?> GetByEmployeeIdAsync(string employeeId, bool includeAdjustments = false, CancellationToken cancellationToken = default)
        {
            IQueryable<EmployeeCompensationAccount> query = _context.EmployeeCompensationAccounts
                .Where(account => account.EmployeeId == employeeId);

            if (includeAdjustments)
            {
                query = query
                    .Include(account => account.Bonuses!)
                    .Include(account => account.Deductions!);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public Task<bool> EmployeeAccountExistsAsync(string employeeId, CancellationToken cancellationToken = default)
        {
            return _context.EmployeeCompensationAccounts.AnyAsync(account => account.EmployeeId == employeeId, cancellationToken);
        }

        public async Task AddAccountAsync(EmployeeCompensationAccount account, CancellationToken cancellationToken = default)
        {
            await _context.EmployeeCompensationAccounts.AddAsync(account, cancellationToken);
        }

        public void UpdateAccount(EmployeeCompensationAccount account)
        {
            _context.EmployeeCompensationAccounts.Update(account);
        }

        public async Task AddBonusesAsync(IEnumerable<EmployeeBonus> bonuses, CancellationToken cancellationToken = default)
        {
            await _context.EmployeeBonuses.AddRangeAsync(bonuses, cancellationToken);
        }

        public async Task AddDeductionsAsync(IEnumerable<EmployeeDeduction> deductions, CancellationToken cancellationToken = default)
        {
            await _context.EmployeeDeductions.AddRangeAsync(deductions, cancellationToken);
        }

        public async Task<(decimal bonuses, decimal deductions)> GetYearToDateTotalsAsync(int accountId, int year, CancellationToken cancellationToken = default)
        {
            var startOfYear = new DateTime(year, 1, 1);
            var endOfYear = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            var bonusTotalTask = _context.EmployeeBonuses
                .Where(bonus => bonus.EmployeeCompensationAccountId == accountId &&
                                bonus.AwardedOn >= startOfYear &&
                                bonus.AwardedOn <= endOfYear)
                .SumAsync(bonus => bonus.Amount, cancellationToken);

            var deductionTotalTask = _context.EmployeeDeductions
                .Where(deduction => deduction.EmployeeCompensationAccountId == accountId &&
                                    deduction.AppliedOn >= startOfYear &&
                                    deduction.AppliedOn <= endOfYear)
                .SumAsync(deduction => deduction.Amount, cancellationToken);

            await Task.WhenAll(bonusTotalTask, deductionTotalTask);
            return (bonusTotalTask.Result, deductionTotalTask.Result);
        }

        public async Task<IEnumerable<EmployeeBonus>> GetRecentBonusesAsync(int accountId, int take, CancellationToken cancellationToken = default)
        {
            return await _context.EmployeeBonuses
                .Where(bonus => bonus.EmployeeCompensationAccountId == accountId)
                .OrderByDescending(bonus => bonus.AwardedOn)
                .ThenByDescending(bonus => bonus.Id)
                .Take(take)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<EmployeeDeduction>> GetRecentDeductionsAsync(int accountId, int take, CancellationToken cancellationToken = default)
        {
            return await _context.EmployeeDeductions
                .Where(deduction => deduction.EmployeeCompensationAccountId == accountId)
                .OrderByDescending(deduction => deduction.AppliedOn)
                .ThenByDescending(deduction => deduction.Id)
                .Take(take)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
