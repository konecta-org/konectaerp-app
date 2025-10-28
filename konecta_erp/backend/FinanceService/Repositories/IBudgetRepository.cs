using FinanceService.Models;

namespace FinanceService.Repositories
{
    public interface IBudgetRepository
    {
        Task<IEnumerable<Budget>> GetAllAsync(int? fiscalYear = null, bool includeLines = false, CancellationToken cancellationToken = default);
        Task<Budget?> GetByIdAsync(int id, bool includeLines = false, CancellationToken cancellationToken = default);
        Task AddAsync(Budget budget, CancellationToken cancellationToken = default);
        Task UpdateLinesAsync(Budget budget, IEnumerable<BudgetLine> newLines, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
