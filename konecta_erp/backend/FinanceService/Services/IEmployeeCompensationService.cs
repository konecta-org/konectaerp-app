using FinanceService.Dtos;

namespace FinanceService.Services
{
    public interface IEmployeeCompensationService
    {
        Task<EmployeeCompensationResponseDto> UpsertAccountAsync(EmployeeAccountUpsertDto request, CancellationToken cancellationToken = default);
        Task<EmployeeCompensationResponseDto?> GetAccountSummaryAsync(string employeeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<EmployeeBonusResponseDto>> AddBonusesAsync(string employeeId, IEnumerable<CompensationBonusCreateDto> bonuses, CancellationToken cancellationToken = default);
        Task<IEnumerable<EmployeeDeductionResponseDto>> AddDeductionsAsync(string employeeId, IEnumerable<CompensationDeductionCreateDto> deductions, CancellationToken cancellationToken = default);
    }
}
