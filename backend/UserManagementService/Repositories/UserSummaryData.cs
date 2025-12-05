namespace UserManagementService.Repositories
{
    public record UserSummaryData(
        int TotalUsers,
        int ActiveUsers,
        int LockedUsers,
        int DeletedUsers,
        IReadOnlyDictionary<string, int> UsersPerRole,
        IReadOnlyDictionary<string, int> UsersPerDepartment);
}
