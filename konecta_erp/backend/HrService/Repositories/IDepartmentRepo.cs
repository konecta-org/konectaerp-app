using HrService.Models;

namespace HrService.Repositories
{
    public interface IDepartmentRepo
    {
        Task<IEnumerable<Department>> GetAllDepartmentsAsync(bool includeEmployees = false);
        Task<Department?> GetDepartmentByIdAsync(Guid id, bool includeEmployees = false);
        Task<bool> DepartmentNameExistsAsync(string departmentName, Guid? excludeDepartmentId = null);
        Task AddDepartmentAsync(Department department);
        Task UpdateDepartmentAsync(Department department);
        Task<bool> DeleteDepartmentAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
