using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HrService.Models
{
    public class Department
    {
        [Key]
        public Guid DepartmentId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public Guid? ManagerId { get; set; }

        public ICollection<Employee>? Employees { get; set; }

        public ICollection<JobOpening>? JobOpenings { get; set; }

        public Employee? Manager { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
