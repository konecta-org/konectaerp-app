namespace UserManagementService.Dtos
{
    public record UserResponseDto(
        string Id,
        string Email,
        string FullName,
        string? FirstName,
        string? LastName,
        string? Department,
        string? JobTitle,
        string Role,
        string Status,
        string? PhoneNumber,
        string? ManagerId,
        bool IsLocked,
        bool IsDeleted,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        DateTime? LastLoginAt,
        DateTime? DeactivatedAt);
}
