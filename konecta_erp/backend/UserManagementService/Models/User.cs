using System;

namespace UserManagementService.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;  
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Department { get; set; }         
        public string Role { get; set; } = "Employee";
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
