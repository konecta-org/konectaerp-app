using HrService.Data;
using HrService.Models;
using Microsoft.EntityFrameworkCore;

namespace HrService.Repositories
{
    public class InterviewRepo : IInterviewRepo
    {
        private readonly AppDbContext _context;

        public InterviewRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Interview>> GetAllAsync()
        {
            return await _context.Interviews
                .Include(i => i.JobApplication!)
                .ThenInclude(a => a.JobOpening)
                .Include(i => i.Interviewer)
                .AsNoTracking()
                .OrderByDescending(i => i.ScheduledAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Interview>> GetByApplicationAsync(Guid jobApplicationId)
        {
            return await _context.Interviews
                .Where(i => i.JobApplicationId == jobApplicationId)
                .Include(i => i.Interviewer)
                .AsNoTracking()
                .OrderBy(i => i.ScheduledAt)
                .ToListAsync();
        }

        public async Task<Interview?> GetByIdAsync(Guid id)
        {
            return await _context.Interviews
                .Include(i => i.JobApplication!)
                .ThenInclude(a => a.JobOpening)
                .Include(i => i.Interviewer)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task AddAsync(Interview interview)
        {
            await _context.Interviews.AddAsync(interview);
        }

        public async Task UpdateAsync(Interview interview)
        {
            var existing = await _context.Interviews.FirstOrDefaultAsync(i => i.Id == interview.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Interview {interview.Id} not found.");
            }

            _context.Entry(existing).CurrentValues.SetValues(interview);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var interview = await _context.Interviews.FirstOrDefaultAsync(i => i.Id == id);
            if (interview == null)
            {
                return false;
            }

            _context.Interviews.Remove(interview);
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
