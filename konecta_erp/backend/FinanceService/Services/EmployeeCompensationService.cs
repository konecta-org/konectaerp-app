using AutoMapper;
using FinanceService.Dtos;
using FinanceService.Models;
using FinanceService.Repositories;
using Microsoft.Extensions.Logging;

namespace FinanceService.Services
{
    public class EmployeeCompensationService : IEmployeeCompensationService
    {
        private readonly IEmployeeCompensationRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeCompensationService> _logger;

        private const int DefaultRecentItems = 5;

        public EmployeeCompensationService(
            IEmployeeCompensationRepository repository,
            IMapper mapper,
            ILogger<EmployeeCompensationService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<EmployeeCompensationResponseDto> UpsertAccountAsync(EmployeeAccountUpsertDto request, CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByEmployeeIdAsync(request.EmployeeId, includeAdjustments: false, cancellationToken);
            if (existing == null)
            {
                var account = _mapper.Map<EmployeeCompensationAccount>(request);
                account.CreatedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;
                await _repository.AddAccountAsync(account, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created compensation account for employee {EmployeeId}", request.EmployeeId);
                return await BuildResponseAsync(account.EmployeeId, cancellationToken);
            }

            _mapper.Map(request, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            _repository.UpdateAccount(existing);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated compensation account for employee {EmployeeId}", request.EmployeeId);
            return await BuildResponseAsync(existing.EmployeeId, cancellationToken);
        }

        public async Task<EmployeeCompensationResponseDto?> GetAccountSummaryAsync(string employeeId, CancellationToken cancellationToken = default)
        {
            var exists = await _repository.EmployeeAccountExistsAsync(employeeId, cancellationToken);
            if (!exists)
            {
                return null;
            }

            return await BuildResponseAsync(employeeId, cancellationToken);
        }

        public async Task<IEnumerable<EmployeeBonusResponseDto>> AddBonusesAsync(string employeeId, IEnumerable<CompensationBonusCreateDto> bonuses, CancellationToken cancellationToken = default)
        {
            var account = await _repository.GetByEmployeeIdAsync(employeeId, includeAdjustments: false, cancellationToken)
                ?? throw new InvalidOperationException($"Compensation account for employee '{employeeId}' does not exist.");

            var bonusList = bonuses.ToList();
            if (bonusList.Count == 0)
            {
                return Array.Empty<EmployeeBonusResponseDto>();
            }

            if (bonusList.Any(bonus => bonus.Amount <= 0))
            {
                throw new ArgumentException("Bonus amounts must be greater than zero.", nameof(bonuses));
            }

            var entities = _mapper.Map<List<EmployeeBonus>>(bonusList);
            foreach (var entity in entities)
            {
                entity.EmployeeCompensationAccountId = account.Id;
                entity.Amount = RoundCurrency(entity.Amount);
            }

            await _repository.AddBonusesAsync(entities, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added {Count} bonuses for employee {EmployeeId}", entities.Count, employeeId);
            return _mapper.Map<IEnumerable<EmployeeBonusResponseDto>>(entities);
        }

        public async Task<IEnumerable<EmployeeDeductionResponseDto>> AddDeductionsAsync(string employeeId, IEnumerable<CompensationDeductionCreateDto> deductions, CancellationToken cancellationToken = default)
        {
            var account = await _repository.GetByEmployeeIdAsync(employeeId, includeAdjustments: false, cancellationToken)
                ?? throw new InvalidOperationException($"Compensation account for employee '{employeeId}' does not exist.");

            var deductionList = deductions.ToList();
            if (deductionList.Count == 0)
            {
                return Array.Empty<EmployeeDeductionResponseDto>();
            }

            if (deductionList.Any(deduction => deduction.Amount <= 0))
            {
                throw new ArgumentException("Deduction amounts must be greater than zero.", nameof(deductions));
            }

            var entities = _mapper.Map<List<EmployeeDeduction>>(deductionList);
            foreach (var entity in entities)
            {
                entity.EmployeeCompensationAccountId = account.Id;
                entity.Amount = RoundCurrency(entity.Amount);
            }

            await _repository.AddDeductionsAsync(entities, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added {Count} deductions for employee {EmployeeId}", entities.Count, employeeId);
            return _mapper.Map<IEnumerable<EmployeeDeductionResponseDto>>(entities);
        }

        private async Task<EmployeeCompensationResponseDto> BuildResponseAsync(string employeeId, CancellationToken cancellationToken)
        {
            var account = await _repository.GetByEmployeeIdAsync(employeeId, includeAdjustments: false, cancellationToken)
                ?? throw new InvalidOperationException($"Compensation account for employee '{employeeId}' could not be found.");

            var result = _mapper.Map<EmployeeCompensationResponseDto>(account);
            var currentYear = DateTime.UtcNow.Year;
            var (bonuses, deductions) = await _repository.GetYearToDateTotalsAsync(account.Id, currentYear, cancellationToken);
            var recentBonuses = await _repository.GetRecentBonusesAsync(account.Id, DefaultRecentItems, cancellationToken);
            var recentDeductions = await _repository.GetRecentDeductionsAsync(account.Id, DefaultRecentItems, cancellationToken);

            var bonusDtos = _mapper.Map<IEnumerable<EmployeeBonusResponseDto>>(recentBonuses).ToList();
            var deductionDtos = _mapper.Map<IEnumerable<EmployeeDeductionResponseDto>>(recentDeductions).ToList();

            var roundedBonuses = RoundCurrency(bonuses);
            var roundedDeductions = RoundCurrency(deductions);
            var net = RoundCurrency(account.BaseSalary + roundedBonuses - roundedDeductions);

            return result with
            {
                TotalBonusesYtd = roundedBonuses,
                TotalDeductionsYtd = roundedDeductions,
                NetCompensationYtd = net,
                RecentBonuses = bonusDtos,
                RecentDeductions = deductionDtos
            };
        }

        private static decimal RoundCurrency(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
