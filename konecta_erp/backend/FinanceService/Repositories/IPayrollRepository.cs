using FinanceService.Models;

namespace FinanceService.Repositories
{
    public interface IPayrollRepository
    {
        Task<IEnumerable<PayrollRun>> GetAllAsync(DateTime? from = null, DateTime? to = null, bool includeEntries = false, CancellationToken cancellationToken = default);
        Task<PayrollRun?> GetByIdAsync(int id, bool includeEntries = false, CancellationToken cancellationToken = default);
        Task AddAsync(PayrollRun payrollRun, CancellationToken cancellationToken = default);
        Task<bool> PayrollNumberExistsAsync(string payrollNumber, int? excludeId = null, CancellationToken cancellationToken = default);
        Task UpdateEntriesAsync(PayrollRun payrollRun, IEnumerable<PayrollEntry> newEntries, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
