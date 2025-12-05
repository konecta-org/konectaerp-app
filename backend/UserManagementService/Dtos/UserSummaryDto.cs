namespace UserManagementService.Dtos
{
    public record UserSummaryDto(
        int TotalUsers,
        int ActiveUsers,
        int LockedUsers,
        int DeletedUsers,
        IReadOnlyDictionary<string, int> UsersPerRole,
        IReadOnlyDictionary<string, int> UsersPerDepartment);
}
