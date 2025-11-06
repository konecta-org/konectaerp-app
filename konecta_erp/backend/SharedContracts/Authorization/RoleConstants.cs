namespace SharedContracts.Authorization;

/// <summary>
/// Canonical role names used across the platform.
/// </summary>
public static class RoleConstants
{
    public const string SystemAdmin = "System Admin";
    public const string HrAdmin = "HR Admin";
    public const string HrStaff = "HR Staff";
    public const string FinanceManager = "Finance Manager";
    public const string FinanceStaff = "Finance Staff";
    public const string DepartmentManager = "Department Manager";
    public const string Employee = "Employee";

    public static IReadOnlyCollection<string> All { get; } = new[]
    {
        SystemAdmin,
        HrAdmin,
        HrStaff,
        FinanceManager,
        FinanceStaff,
        DepartmentManager,
        Employee
    };
}
