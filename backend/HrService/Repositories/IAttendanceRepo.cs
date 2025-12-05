using HrService.Models;

namespace HrService.Repositories
{
    public interface IAttendanceRepo
    {
        Task<IEnumerable<AttendanceRecord>> GetAllAsync();
        Task<IEnumerable<AttendanceRecord>> GetByEmployeeAsync(Guid employeeId, DateTime? startDate = null, DateTime? endDate = null);
        Task<AttendanceRecord?> GetByIdAsync(Guid id);
        Task<AttendanceRecord?> GetByEmployeeAndDateAsync(Guid employeeId, DateTime workDate);
        Task AddAsync(AttendanceRecord record);
        Task UpdateAsync(AttendanceRecord record);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
