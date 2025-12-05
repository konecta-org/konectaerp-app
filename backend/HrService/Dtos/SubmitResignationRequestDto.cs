using System;
using System.ComponentModel.DataAnnotations;

namespace HrService.Dtos
{
    public class SubmitResignationRequestDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        [MaxLength(2000)]
        public string? Reason { get; set; }
    }
}
