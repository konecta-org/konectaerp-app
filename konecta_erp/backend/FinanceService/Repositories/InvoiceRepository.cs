using FinanceService.Data;
using FinanceService.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync(string? statusFilter = null, bool includeLines = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Invoice> query = _context.Invoices.AsQueryable();

            if (includeLines)
            {
                query = query.Include(invoice => invoice.Lines);
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(invoice => invoice.Status == statusFilter);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(invoice => invoice.IssueDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Invoice?> GetByIdAsync(int id, bool includeLines = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Invoice> query = _context.Invoices.Where(invoice => invoice.Id == id);

            if (includeLines)
            {
                query = query.Include(invoice => invoice.Lines);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            await _context.Invoices.AddAsync(invoice, cancellationToken);
        }

        public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Invoice> query = _context.Invoices.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(invoice => invoice.Id != excludeId.Value);
            }

            return await query.AnyAsync(invoice => invoice.InvoiceNumber == invoiceNumber, cancellationToken);
        }

        public async Task UpdateLinesAsync(Invoice invoice, IEnumerable<InvoiceLine> newLines, CancellationToken cancellationToken = default)
        {
            var existingLines = await _context.InvoiceLines
                .Where(line => line.InvoiceId == invoice.Id)
                .ToListAsync(cancellationToken);

            if (existingLines.Count > 0)
            {
                _context.InvoiceLines.RemoveRange(existingLines);
            }

            foreach (var line in newLines)
            {
                line.InvoiceId = invoice.Id;
            }

            await _context.InvoiceLines.AddRangeAsync(newLines, cancellationToken);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
            if (invoice == null)
            {
                return false;
            }

            _context.Invoices.Remove(invoice);
            return true;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
