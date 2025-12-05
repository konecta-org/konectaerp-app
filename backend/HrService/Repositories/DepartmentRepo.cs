using HrService.Data;
using HrService.Models;
using Microsoft.EntityFrameworkCore;

namespace HrService.Repositories
{
    public class DepartmentRepo : IDepartmentRepo
    {
        private readonly AppDbContext _context;

        public DepartmentRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync(bool includeEmployees = false)
        {
            IQueryable<Department> query = _context.Departments;

            if (includeEmployees)
            {
                query = query.Include(d => d.Employees);
            }

            return await query
                .OrderBy(d => d.DepartmentName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Department?> GetDepartmentByIdAsync(Guid id, bool includeEmployees = false)
        {
            IQueryable<Department> query = _context.Departments;

            if (includeEmployees)
            {
                query = query.Include(d => d.Employees);
            }

            return await query.FirstOrDefaultAsync(d => d.DepartmentId == id);
        }

        public async Task<bool> DepartmentNameExistsAsync(string departmentName, Guid? excludeDepartmentId = null)
        {
            var query = _context.Departments.AsQueryable();

            if (excludeDepartmentId.HasValue)
            {
                query = query.Where(d => d.DepartmentId != excludeDepartmentId.Value);
            }

            return await query.AnyAsync(d => d.DepartmentName == departmentName);
        }

        public async Task AddDepartmentAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            var existing = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == department.DepartmentId);
            if (existing == null)
            {
                throw new InvalidOperationException($"Department {department.DepartmentId} not found.");
            }

            _context.Entry(existing).CurrentValues.SetValues(department);
        }

        public async Task<bool> DeleteDepartmentAsync(Guid id)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (department == null)
            {
                return false;
            }

            _context.Departments.Remove(department);
            return true;
        }

        public async Task<bool> AssignManagerAsync(Guid departmentId, Guid employeeId)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
            if (department == null)
            {
                return false;
            }

            department.ManagerId = employeeId;
            department.UpdatedAt = DateTime.UtcNow;
            _context.Departments.Update(department);
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
