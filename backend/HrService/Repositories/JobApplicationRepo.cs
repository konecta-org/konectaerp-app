using HrService.Data;
using HrService.Models;
using Microsoft.EntityFrameworkCore;

namespace HrService.Repositories
{
    public class JobApplicationRepo : IJobApplicationRepo
    {
        private readonly AppDbContext _context;

        public JobApplicationRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobApplication>> GetAllAsync()
        {
            return await _context.JobApplications
                .Include(a => a.JobOpening)
                .AsNoTracking()
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobApplication>> GetByJobOpeningAsync(Guid jobOpeningId)
        {
            return await _context.JobApplications
                .Where(a => a.JobOpeningId == jobOpeningId)
                .Include(a => a.JobOpening)
                .AsNoTracking()
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task<JobApplication?> GetByIdAsync(Guid id, bool includeDetails = false)
        {
            IQueryable<JobApplication> query = _context.JobApplications
                .Include(a => a.JobOpening);

            if (includeDetails)
            {
                query = query.Include(a => a.Interviews!)
                             .ThenInclude(i => i.Interviewer);
            }

            return await query.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(JobApplication application)
        {
            await _context.JobApplications.AddAsync(application);
        }

        public async Task UpdateAsync(JobApplication application)
        {
            var existing = await _context.JobApplications.FirstOrDefaultAsync(a => a.Id == application.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Job application {application.Id} not found.");
            }

            _context.Entry(existing).CurrentValues.SetValues(application);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var application = await _context.JobApplications.FirstOrDefaultAsync(a => a.Id == id);
            if (application == null)
            {
                return false;
            }

            _context.JobApplications.Remove(application);
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
