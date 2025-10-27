using HrService.Models;

namespace HrService.Repositories
{
    public interface ILeaveRequestRepo
    {
        Task<IEnumerable<LeaveRequest>> GetAllAsync();
        Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId);
        Task<IEnumerable<LeaveRequest>> GetPendingAsync();
        Task<LeaveRequest?> GetByIdAsync(Guid id);
        Task AddAsync(LeaveRequest leaveRequest);
        Task UpdateAsync(LeaveRequest leaveRequest);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
