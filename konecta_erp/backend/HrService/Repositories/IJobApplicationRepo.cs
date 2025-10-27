using HrService.Models;

namespace HrService.Repositories
{
    public interface IJobApplicationRepo
    {
        Task<IEnumerable<JobApplication>> GetAllAsync();
        Task<IEnumerable<JobApplication>> GetByJobOpeningAsync(Guid jobOpeningId);
        Task<JobApplication?> GetByIdAsync(Guid id, bool includeDetails = false);
        Task AddAsync(JobApplication application);
        Task UpdateAsync(JobApplication application);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
