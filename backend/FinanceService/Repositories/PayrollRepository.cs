using FinanceService.Data;
using FinanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Repositories
{
    public class PayrollRepository : IPayrollRepository
    {
        private readonly AppDbContext _context;

        public PayrollRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PayrollRun>> GetAllAsync(DateTime? from = null, DateTime? to = null, bool includeEntries = false, CancellationToken cancellationToken = default)
        {
            IQueryable<PayrollRun> query = _context.PayrollRuns;

            if (from.HasValue)
            {
                query = query.Where(run => run.PeriodEnd >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(run => run.PeriodStart <= to.Value);
            }

            if (includeEntries)
            {
                query = query.Include(run => run.Entries);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(run => run.PaymentDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<PayrollRun?> GetByIdAsync(int id, bool includeEntries = false, CancellationToken cancellationToken = default)
        {
            IQueryable<PayrollRun> query = _context.PayrollRuns.Where(run => run.Id == id);

            if (includeEntries)
            {
                query = query.Include(run => run.Entries);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(PayrollRun payrollRun, CancellationToken cancellationToken = default)
        {
            await _context.PayrollRuns.AddAsync(payrollRun, cancellationToken);
        }

        public async Task<bool> PayrollNumberExistsAsync(string payrollNumber, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            IQueryable<PayrollRun> query = _context.PayrollRuns.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(run => run.Id != excludeId.Value);
            }

            return await query.AnyAsync(run => run.PayrollNumber == payrollNumber, cancellationToken);
        }

        public async Task UpdateEntriesAsync(PayrollRun payrollRun, IEnumerable<PayrollEntry> newEntries, CancellationToken cancellationToken = default)
        {
            var existingEntries = await _context.PayrollEntries
                .Where(entry => entry.PayrollRunId == payrollRun.Id)
                .ToListAsync(cancellationToken);

            if (existingEntries.Count > 0)
            {
                _context.PayrollEntries.RemoveRange(existingEntries);
            }

            foreach (var entry in newEntries)
            {
                entry.PayrollRunId = payrollRun.Id;
            }

            await _context.PayrollEntries.AddRangeAsync(newEntries, cancellationToken);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var payrollRun = await _context.PayrollRuns.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
            if (payrollRun == null)
            {
                return false;
            }

            _context.PayrollRuns.Remove(payrollRun);
            return true;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
