using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Dtos
{
    public class UserStatusUpdateDto
    {
        [Required]
        [MaxLength(32)]
        public string Status { get; set; } = "Active";

        public bool LockAccount { get; set; }

        [MaxLength(256)]
        public string? ChangedBy { get; set; }
    }
}
