using HrService.Data;
using HrService.Models;
using Microsoft.EntityFrameworkCore;

namespace HrService.Repositories
{
    public class LeaveRequestRepo : ILeaveRequestRepo
    {
        private readonly AppDbContext _context;

        public LeaveRequestRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.ApprovedBy)
                .AsNoTracking()
                .OrderByDescending(l => l.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(Guid employeeId)
        {
            return await _context.LeaveRequests
                .Where(l => l.EmployeeId == employeeId)
                .Include(l => l.Employee)
                .Include(l => l.ApprovedBy)
                .AsNoTracking()
                .OrderByDescending(l => l.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingAsync()
        {
            return await _context.LeaveRequests
                .Where(l => l.Status == LeaveStatus.Pending)
                .Include(l => l.Employee)
                .AsNoTracking()
                .OrderBy(l => l.StartDate)
                .ToListAsync();
        }

        public async Task<LeaveRequest?> GetByIdAsync(Guid id)
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.ApprovedBy)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task AddAsync(LeaveRequest leaveRequest)
        {
            await _context.LeaveRequests.AddAsync(leaveRequest);
        }

        public async Task UpdateAsync(LeaveRequest leaveRequest)
        {
            var existing = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == leaveRequest.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Leave request {leaveRequest.Id} not found.");
            }

            _context.Entry(existing).CurrentValues.SetValues(leaveRequest);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var leave = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null)
            {
                return false;
            }

            _context.LeaveRequests.Remove(leave);
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
