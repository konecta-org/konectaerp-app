using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceService.Models
{
    public class EmployeeCompensationAccount
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string EmployeeId { get; set; } = default!;

        [Required]
        [MaxLength(128)]
        public string EmployeeName { get; set; } = default!;

        [MaxLength(64)]
        public string? EmployeeNumber { get; set; }

        [MaxLength(128)]
        public string? Department { get; set; }

        [MaxLength(128)]
        public string? JobTitle { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        [MaxLength(16)]
        public string Currency { get; set; } = "USD";

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;

        [MaxLength(128)]
        public string? BankName { get; set; }

        [MaxLength(64)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(64)]
        public string? BankRoutingNumber { get; set; }

        [MaxLength(64)]
        public string? Iban { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<EmployeeBonus>? Bonuses { get; set; }

        public ICollection<EmployeeDeduction>? Deductions { get; set; }
    }
}
