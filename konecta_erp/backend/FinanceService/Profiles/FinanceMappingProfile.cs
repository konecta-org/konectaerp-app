using AutoMapper;
using FinanceService.Dtos;
using FinanceService.Models;

namespace FinanceService.Profiles
{
    public class FinanceMappingProfile : Profile
    {
        public FinanceMappingProfile()
        {
            // Invoice mappings
            CreateMap<Invoice, InvoiceResponseDto>()
                .ForMember(dest => dest.BalanceDue, opt => opt.MapFrom(src => src.TotalAmount - src.PaidAmount))
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines ?? new List<InvoiceLine>()));

            CreateMap<InvoiceLine, InvoiceLineResponseDto>();

            CreateMap<InvoiceUpsertDto, Invoice>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Lines, opt => opt.Ignore());

            CreateMap<InvoiceLineUpsertDto, InvoiceLine>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));

            // Expense mappings
            CreateMap<Expense, ExpenseResponseDto>();
            CreateMap<ExpenseUpsertDto, Expense>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Budget mappings
            CreateMap<Budget, BudgetResponseDto>()
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines ?? new List<BudgetLine>()))
                .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.TotalAmount - src.SpentAmount));

            CreateMap<BudgetLine, BudgetLineResponseDto>();
            CreateMap<BudgetUpsertDto, Budget>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Lines, opt => opt.Ignore());

            CreateMap<BudgetLineUpsertDto, BudgetLine>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Payroll mappings
            CreateMap<PayrollRun, PayrollRunResponseDto>()
                .ForMember(dest => dest.Entries, opt => opt.MapFrom(src => src.Entries ?? new List<PayrollEntry>()));

            CreateMap<PayrollEntry, PayrollEntryResponseDto>();

            CreateMap<PayrollRunUpsertDto, PayrollRun>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Entries, opt => opt.Ignore());

            CreateMap<PayrollEntryUpsertDto, PayrollEntry>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
