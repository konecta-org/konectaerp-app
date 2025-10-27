using HrService.Models;

namespace HrService.Repositories
{
    public interface IInterviewRepo
    {
        Task<IEnumerable<Interview>> GetAllAsync();
        Task<IEnumerable<Interview>> GetByApplicationAsync(Guid jobApplicationId);
        Task<Interview?> GetByIdAsync(Guid id);
        Task AddAsync(Interview interview);
        Task UpdateAsync(Interview interview);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
