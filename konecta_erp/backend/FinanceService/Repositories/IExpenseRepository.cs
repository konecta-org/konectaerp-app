using FinanceService.Models;

namespace FinanceService.Repositories
{
    public interface IExpenseRepository
    {
        Task<IEnumerable<Expense>> GetAllAsync(string? category = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
        Task<Expense?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Expense expense, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
