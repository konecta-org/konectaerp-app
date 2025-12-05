using FinanceService.Data;
using FinanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly AppDbContext _context;

        public BudgetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Budget>> GetAllAsync(int? fiscalYear = null, bool includeLines = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Budget> query = _context.Budgets;

            if (fiscalYear.HasValue)
            {
                query = query.Where(budget => budget.FiscalYear == fiscalYear.Value);
            }

            if (includeLines)
            {
                query = query.Include(budget => budget.Lines);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(budget => budget.FiscalYear)
                .ThenBy(budget => budget.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Budget?> GetByIdAsync(int id, bool includeLines = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Budget> query = _context.Budgets.Where(budget => budget.Id == id);

            if (includeLines)
            {
                query = query.Include(budget => budget.Lines);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(Budget budget, CancellationToken cancellationToken = default)
        {
            await _context.Budgets.AddAsync(budget, cancellationToken);
        }

        public async Task UpdateLinesAsync(Budget budget, IEnumerable<BudgetLine> newLines, CancellationToken cancellationToken = default)
        {
            var existingLines = await _context.BudgetLines
                .Where(line => line.BudgetId == budget.Id)
                .ToListAsync(cancellationToken);

            if (existingLines.Count > 0)
            {
                _context.BudgetLines.RemoveRange(existingLines);
            }

            foreach (var line in newLines)
            {
                line.BudgetId = budget.Id;
            }

            await _context.BudgetLines.AddRangeAsync(newLines, cancellationToken);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
            if (budget == null)
            {
                return false;
            }

            _context.Budgets.Remove(budget);
            return true;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
