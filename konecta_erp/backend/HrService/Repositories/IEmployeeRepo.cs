using HrService.Models;

namespace HrService.Repositories
{
    public interface IEmployeeRepo
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(Guid id, bool includeDepartment = false);
        Task<bool> WorkEmailExistsAsync(string workEmail, Guid? excludeEmployeeId = null);
        Task AddEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(Employee employee);
        Task<bool> UpdateEmployeeIdentityAsync(Guid employeeId, Guid identityUserId);
        Task<bool> RecordEmployeeExitAsync(Guid employeeId, DateTime exitDate, EmploymentStatus exitStatus, string? reason, bool? eligibleForRehire);
        Task<bool> DeleteEmployeeAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
