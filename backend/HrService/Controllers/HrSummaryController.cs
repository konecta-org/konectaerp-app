using AutoMapper;
using HrService.Dtos;
using HrService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HrService.Models;

namespace HrService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HrSummaryController : ControllerBase
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IDepartmentRepo _departmentRepo;
        private readonly IResignationRequestRepo _resignationRepo;

        public HrSummaryController(
            IEmployeeRepo employeeRepo,
            IDepartmentRepo departmentRepo,
            IResignationRequestRepo resignationRepo)
        {
            _employeeRepo = employeeRepo;
            _departmentRepo = departmentRepo;
            _resignationRepo = resignationRepo;
        }

        [HttpGet]
        public async Task<ActionResult<HrSummaryDto>> GetSummary(CancellationToken cancellationToken = default)
        {

            var employees = (await _employeeRepo.GetAllEmployeesAsync()).ToList();
            var departments = (await _departmentRepo.GetAllDepartmentsAsync(false)).ToList();
            var pendingResignations = (await _resignationRepo.GetAllAsync(ResignationStatus.Pending)).ToList();

            var totalEmployees = employees.Count;
            var activeEmployees = employees.Count(e => e.Status == Models.EmploymentStatus.Active);
            var deptCount = departments.Count;
            var pendingCount = pendingResignations.Count;

            var dto = new HrSummaryDto(totalEmployees, activeEmployees, deptCount, pendingCount);
            return Ok(dto);
        }
    }
}
