using HrService.Data;
using HrService.Models;
using Microsoft.EntityFrameworkCore;

namespace HrService.Repositories
{
    public class JobOpeningRepo : IJobOpeningRepo
    {
        private readonly AppDbContext _context;

        public JobOpeningRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobOpening>> GetAllAsync(bool includeApplications = false)
        {
            IQueryable<JobOpening> query = _context.JobOpenings
                .Include(j => j.Department);

            if (includeApplications)
            {
                query = query.Include(j => j.Applications!);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();
        }

        public async Task<JobOpening?> GetByIdAsync(Guid id, bool includeApplications = false)
        {
            IQueryable<JobOpening> query = _context.JobOpenings
                .Include(j => j.Department);

            if (includeApplications)
            {
                query = query.Include(j => j.Applications!)
                             .ThenInclude(a => a.Interviews!);
            }

            return await query.FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<IEnumerable<JobOpening>> GetByDepartmentAsync(Guid departmentId)
        {
            return await _context.JobOpenings
                .Where(j => j.DepartmentId == departmentId)
                .Include(j => j.Department)
                .AsNoTracking()
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();
        }

        public async Task AddAsync(JobOpening jobOpening)
        {
            await _context.JobOpenings.AddAsync(jobOpening);
        }

        public async Task UpdateAsync(JobOpening jobOpening)
        {
            var existing = await _context.JobOpenings.FirstOrDefaultAsync(j => j.Id == jobOpening.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Job opening {jobOpening.Id} not found.");
            }

            _context.Entry(existing).CurrentValues.SetValues(jobOpening);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var job = await _context.JobOpenings.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null)
            {
                return false;
            }

            _context.JobOpenings.Remove(job);
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
