using HrService.Data;
using HrService.Models;
using Microsoft.EntityFrameworkCore;

namespace HrService.Repositories
{
    public class EmployeeRepo : IEmployeeRepo
    {
        private readonly AppDbContext _context;

        public EmployeeRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .AsNoTracking()
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(Guid id, bool includeDepartment = false)
        {
            IQueryable<Employee> query = _context.Employees;

            if (includeDepartment)
            {
                query = query.Include(e => e.Department);
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> WorkEmailExistsAsync(string workEmail, Guid? excludeEmployeeId = null)
        {
            var query = _context.Employees.AsQueryable();

            if (excludeEmployeeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeEmployeeId.Value);
            }

            return await query.AnyAsync(e => e.WorkEmail == workEmail);
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employee.Id);
            if (existingEmployee == null)
            {
                throw new InvalidOperationException($"Employee {employee.Id} not found.");
            }

            _context.Entry(existingEmployee).CurrentValues.SetValues(employee);
        }

        public async Task<bool> TerminateEmployeeAsync(Guid id, string? reason, bool? eligibleForRehire)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return false;
            }

            employee.Status = EmploymentStatus.Terminated;
            employee.ExitDate = DateTime.UtcNow;
            employee.ExitReason = reason;
            employee.EligibleForRehire = eligibleForRehire;
            employee.UpdatedAt = DateTime.UtcNow;

            _context.Employees.Update(employee);
            return true;
        }

        public async Task<bool> UpdateEmployeeIdentityAsync(Guid employeeId, Guid identityUserId)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (employee == null)
            {
                return false;
            }

            if (employee.UserId == identityUserId)
            {
                return true;
            }

            employee.UserId = identityUserId;
            employee.UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public async Task<bool> RecordEmployeeExitAsync(Guid employeeId, DateTime exitDate, EmploymentStatus exitStatus, string? reason, bool? eligibleForRehire)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (employee == null)
            {
                return false;
            }

            employee.ExitDate = exitDate;
            employee.ExitReason = reason;
            employee.EligibleForRehire = eligibleForRehire;
            employee.Status = exitStatus;
            employee.UpdatedAt = DateTime.UtcNow;

            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
