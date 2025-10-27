using System;
using System.ComponentModel.DataAnnotations;

namespace HrService.Dtos
{
    public class UpdateDepartmentDto
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required, MaxLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public Guid? ManagerId { get; set; }
    }
}
