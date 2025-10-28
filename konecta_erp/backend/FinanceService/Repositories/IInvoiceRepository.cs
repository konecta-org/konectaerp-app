using FinanceService.Models;

namespace FinanceService.Repositories
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetAllAsync(string? statusFilter = null, bool includeLines = false, CancellationToken cancellationToken = default);
        Task<Invoice?> GetByIdAsync(int id, bool includeLines = false, CancellationToken cancellationToken = default);
        Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
        Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, int? excludeId = null, CancellationToken cancellationToken = default);
        Task UpdateLinesAsync(Invoice invoice, IEnumerable<InvoiceLine> newLines, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
