using System;
using System.Collections.Generic;

namespace UserManagementService.Dtos
{
    public record UserAssignedRoleDto(int RoleId, string Name, bool IsSystemDefault);

    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public string PrimaryRole { get; set; } = "Unassigned";
        public IReadOnlyCollection<UserAssignedRoleDto> Roles { get; set; } = Array.Empty<UserAssignedRoleDto>();
        public string Status { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ManagerId { get; set; }
        public bool IsLocked { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }
}
