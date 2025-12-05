using HrService.Data;
using HrService.Models;
using Microsoft.EntityFrameworkCore;

namespace HrService.Repositories
{
    public class ResignationRequestRepo : IResignationRequestRepo
    {
        private readonly AppDbContext _context;

        public ResignationRequestRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ResignationRequest>> GetAllAsync(ResignationStatus? status = null)
        {
            var query = _context.ResignationRequests
                .Include(r => r.Employee)
                .Include(r => r.ApprovedBy)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ResignationRequest>> GetByEmployeeAsync(Guid employeeId)
        {
            return await _context.ResignationRequests
                .Where(r => r.EmployeeId == employeeId)
                .Include(r => r.ApprovedBy)
                .AsNoTracking()
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<ResignationRequest?> GetByIdAsync(Guid id, bool includeEmployee = false)
        {
            IQueryable<ResignationRequest> query = _context.ResignationRequests;

            if (includeEmployee)
            {
                query = query.Include(r => r.Employee)
                             .Include(r => r.ApprovedBy);
            }

            return await query.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> HasPendingRequestAsync(Guid employeeId)
        {
            return await _context.ResignationRequests.AnyAsync(r =>
                r.EmployeeId == employeeId && r.Status == ResignationStatus.Pending);
        }

        public async Task AddAsync(ResignationRequest request)
        {
            await _context.ResignationRequests.AddAsync(request);
        }

        public async Task UpdateAsync(ResignationRequest request)
        {
            var existing = await _context.ResignationRequests.FirstOrDefaultAsync(r => r.Id == request.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Resignation request {request.Id} not found.");
            }

            _context.Entry(existing).CurrentValues.SetValues(request);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
