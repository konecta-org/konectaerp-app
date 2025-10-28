using AutoMapper;
using HrService.Dtos;
using HrService.Models;

namespace HrService.Profiles
{
    public class HrMappingProfile : Profile
    {
        public HrMappingProfile()
        {
            CreateMap<AddEmployeeDto, Employee>()
                .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.HireDate ?? DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateEmployeeDto, Employee>()
                .ForMember(dest => dest.HireDate, opt => opt.Condition(src => src.HireDate.HasValue))
                .ForMember(dest => dest.UserId, opt => opt.Condition(src => src.UserId.HasValue))
                .ForMember(dest => dest.ExitDate, opt => opt.Condition(src => src.ExitDate.HasValue))
                .ForMember(dest => dest.ExitReason, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.ExitReason)))
                .ForMember(dest => dest.EligibleForRehire, opt => opt.Condition(src => src.EligibleForRehire.HasValue))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Employee, EmployeeResponseDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.DepartmentName : string.Empty));

            CreateMap<Employee, EmployeeSummaryDto>();

            CreateMap<CreateDepartmentDto, Department>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateDepartmentDto, Department>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Department, DepartmentResponseDto>()
                .ForMember(dest => dest.ManagerFullName, opt => opt.MapFrom(src => src.Manager != null ? src.Manager.FullName : null))
                .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees != null ? src.Employees.Count : 0))
                .ForMember(dest => dest.Employees, opt => opt.MapFrom(src => src.Employees ?? Array.Empty<Employee>()));

            CreateMap<CreateJobOpeningDto, JobOpening>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PostedDate, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateJobOpeningDto, JobOpening>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<JobOpening, JobOpeningResponseDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.DepartmentName : null))
                .ForMember(dest => dest.ApplicationCount, opt => opt.MapFrom(src => src.Applications != null ? src.Applications.Count : 0));

            CreateMap<CreateJobApplicationDto, JobApplication>()
                .ForMember(dest => dest.AppliedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateJobApplicationDto, JobApplication>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<JobApplication, JobApplicationResponseDto>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobOpening != null ? src.JobOpening.Title : string.Empty));

            CreateMap<ScheduleInterviewDto, Interview>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateInterviewDto, Interview>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<Interview, InterviewResponseDto>()
                .ForMember(dest => dest.CandidateName, opt => opt.MapFrom(src => src.JobApplication != null ? src.JobApplication.CandidateName : string.Empty))
                .ForMember(dest => dest.InterviewerName, opt => opt.MapFrom(src => src.Interviewer != null ? src.Interviewer.FullName : null));

            CreateMap<CreateLeaveRequestDto, LeaveRequest>()
                .ForMember(dest => dest.RequestedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateLeaveRequestDto, LeaveRequest>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<LeaveRequest, LeaveRequestResponseDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.FullName : null));

            CreateMap<CreateAttendanceRecordDto, AttendanceRecord>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateAttendanceRecordDto, AttendanceRecord>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<AttendanceRecord, AttendanceRecordResponseDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty));

            CreateMap<SubmitResignationRequestDto, ResignationRequest>()
                .ForMember(dest => dest.RequestedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => ResignationStatus.Pending))
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason));

            CreateMap<ResignationRequest, ResignationRequestResponseDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
                .ForMember(dest => dest.EmployeeEmail, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.WorkEmail : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.FullName : null));
        }
    }
}
