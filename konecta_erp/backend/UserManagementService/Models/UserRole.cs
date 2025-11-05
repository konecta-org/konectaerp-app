namespace UserManagementService.Models
{
    public class UserRole
    {
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = default!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public string? AssignedBy { get; set; }
    }
}
