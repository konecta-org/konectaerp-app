using System;
using System.Collections.Generic;

namespace HrService.Dtos
{
    public class DepartmentResponseDto
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ManagerId { get; set; }
        public string? ManagerFullName { get; set; }
        public int EmployeeCount { get; set; }
        public IReadOnlyCollection<EmployeeSummaryDto> Employees { get; set; } = Array.Empty<EmployeeSummaryDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
