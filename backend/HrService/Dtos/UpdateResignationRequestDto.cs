using System;
using System.ComponentModel.DataAnnotations;

namespace HrService.Dtos
{
    public class UpdateResignationRequestDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        [MaxLength(2000)]
        public string? Reason { get; set; }
    }
}
