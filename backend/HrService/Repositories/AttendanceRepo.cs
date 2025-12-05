using HrService.Data;
using HrService.Models;
using Microsoft.EntityFrameworkCore;

namespace HrService.Repositories
{
    public class AttendanceRepo : IAttendanceRepo
    {
        private readonly AppDbContext _context;

        public AttendanceRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AttendanceRecord>> GetAllAsync()
        {
            return await _context.AttendanceRecords
                .Include(a => a.Employee)
                .AsNoTracking()
                .OrderByDescending(a => a.WorkDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetByEmployeeAsync(Guid employeeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AttendanceRecords
                .Where(a => a.EmployeeId == employeeId)
                .Include(a => a.Employee)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(a => a.WorkDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.WorkDate <= endDate.Value);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(a => a.WorkDate)
                .ToListAsync();
        }

        public async Task<AttendanceRecord?> GetByIdAsync(Guid id)
        {
            return await _context.AttendanceRecords
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AttendanceRecord?> GetByEmployeeAndDateAsync(Guid employeeId, DateTime workDate)
        {
            var dateOnly = workDate.Date;
            return await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.WorkDate.Date == dateOnly);
        }

        public async Task AddAsync(AttendanceRecord record)
        {
            await _context.AttendanceRecords.AddAsync(record);
        }

        public async Task UpdateAsync(AttendanceRecord record)
        {
            var existing = await _context.AttendanceRecords.FirstOrDefaultAsync(a => a.Id == record.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Attendance record {record.Id} not found.");
            }

            _context.Entry(existing).CurrentValues.SetValues(record);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var record = await _context.AttendanceRecords.FirstOrDefaultAsync(a => a.Id == id);
            if (record == null)
            {
                return false;
            }

            _context.AttendanceRecords.Remove(record);
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
