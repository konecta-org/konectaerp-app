using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceService.Models
{
    public class EmployeeBonus
    {
        public int Id { get; set; }

        public int EmployeeCompensationAccountId { get; set; }

        [Required]
        [MaxLength(64)]
        public string BonusType { get; set; } = "General";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime AwardedOn { get; set; } = DateTime.UtcNow;

        [MaxLength(32)]
        public string? Period { get; set; }

        [MaxLength(256)]
        public string? Reference { get; set; }

        [MaxLength(128)]
        public string? AwardedBy { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }

        [MaxLength(64)]
        public string? SourceSystem { get; set; }

        public EmployeeCompensationAccount? Account { get; set; }
    }
}
