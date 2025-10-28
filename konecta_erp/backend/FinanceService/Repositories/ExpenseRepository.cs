using FinanceService.Data;
using FinanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly AppDbContext _context;

        public ExpenseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Expense>> GetAllAsync(string? category = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Expense> query = _context.Expenses;

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(expense => expense.Category == category);
            }

            if (from.HasValue)
            {
                query = query.Where(expense => expense.IncurredOn >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(expense => expense.IncurredOn <= to.Value);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(expense => expense.IncurredOn)
                .ToListAsync(cancellationToken);
        }

        public Task<Expense?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Expenses.FirstOrDefaultAsync(expense => expense.Id == id, cancellationToken);
        }

        public async Task AddAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            await _context.Expenses.AddAsync(expense, cancellationToken);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var expense = await _context.Expenses.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
            if (expense == null)
            {
                return false;
            }

            _context.Expenses.Remove(expense);
            return true;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
