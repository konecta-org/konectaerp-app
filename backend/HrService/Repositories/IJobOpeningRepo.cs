using HrService.Models;

namespace HrService.Repositories
{
    public interface IJobOpeningRepo
    {
        Task<IEnumerable<JobOpening>> GetAllAsync(bool includeApplications = false);
        Task<JobOpening?> GetByIdAsync(Guid id, bool includeApplications = false);
        Task<IEnumerable<JobOpening>> GetByDepartmentAsync(Guid departmentId);
        Task AddAsync(JobOpening jobOpening);
        Task UpdateAsync(JobOpening jobOpening);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
