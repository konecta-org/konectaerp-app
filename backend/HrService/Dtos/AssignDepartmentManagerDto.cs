using System;
using System.ComponentModel.DataAnnotations;

namespace HrService.Dtos
{
    public class AssignDepartmentManagerDto
    {
        [Required]
        public Guid EmployeeId { get; set; }
    }
}
