using HrService.Models;

namespace HrService.Repositories
{
    public interface IResignationRequestRepo
    {
        Task<IEnumerable<ResignationRequest>> GetAllAsync(ResignationStatus? status = null);
        Task<IEnumerable<ResignationRequest>> GetByEmployeeAsync(Guid employeeId);
        Task<ResignationRequest?> GetByIdAsync(Guid id, bool includeEmployee = false);
        Task<bool> HasPendingRequestAsync(Guid employeeId);
        Task AddAsync(ResignationRequest request);
        Task UpdateAsync(ResignationRequest request);
        Task<bool> SaveChangesAsync();
    }
}
