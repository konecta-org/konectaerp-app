namespace SharedContracts.Authorization;

/// <summary>
/// Central catalogue for all application-level permissions.
/// Permission names follow the pattern `{domain}.{resource}.{action}` using lower-case segments.
/// </summary>
public static class PermissionConstants
{
    public const string ClaimType = "permission";

    public static class Finance
    {
        public const string BudgetsRead = "finance.budgets.read";
        public const string BudgetsManage = "finance.budgets.manage";

        public const string ExpensesRead = "finance.expenses.read";
        public const string ExpensesManage = "finance.expenses.manage";

        public const string InvoicesRead = "finance.invoices.read";
        public const string InvoicesManage = "finance.invoices.manage";

        public const string PayrollRead = "finance.payroll.read";
        public const string PayrollManage = "finance.payroll.manage";

        public const string CompensationRead = "finance.compensation.read";
        public const string CompensationManage = "finance.compensation.manage";

        public const string SummaryView = "finance.summary.view";
    }

    public static class Hr
    {
        public const string EmployeesRead = "hr.employees.read";
        public const string EmployeesManage = "hr.employees.manage";

        public const string AttendanceRead = "hr.attendance.read";
        public const string AttendanceManage = "hr.attendance.manage";

        public const string DepartmentsRead = "hr.departments.read";
        public const string DepartmentsManage = "hr.departments.manage";

        public const string LeavesRead = "hr.leaves.read";
        public const string LeavesManage = "hr.leaves.manage";

        public const string JobOpeningsRead = "hr.job-openings.read";
        public const string JobOpeningsManage = "hr.job-openings.manage";

        public const string JobApplicationsRead = "hr.job-applications.read";
        public const string JobApplicationsManage = "hr.job-applications.manage";

        public const string InterviewsRead = "hr.interviews.read";
        public const string InterviewsManage = "hr.interviews.manage";

        public const string ResignationsRead = "hr.resignations.read";
        public const string ResignationsManage = "hr.resignations.manage";

        public const string SummaryView = "hr.summary.view";
    }

    public static class Inventory
    {
        public const string ItemsRead = "inventory.items.read";
        public const string ItemsManage = "inventory.items.manage";

        public const string WarehousesRead = "inventory.warehouses.read";
        public const string WarehousesManage = "inventory.warehouses.manage";

        public const string StockRead = "inventory.stock.read";
        public const string StockManage = "inventory.stock.manage";

        public const string SummaryView = "inventory.summary.view";
    }

    public static class Reporting
    {
        public const string OverviewView = "reporting.overview.view";
        public const string FinanceView = "reporting.finance.view";
        public const string HrView = "reporting.hr.view";
        public const string InventoryView = "reporting.inventory.view";
        public const string ExportPdf = "reporting.export.pdf";
        public const string ExportExcel = "reporting.export.excel";
    }

    public static class UserManagement
    {
        public const string UsersRead = "user-management.users.read";
        public const string UsersManage = "user-management.users.manage";
        public const string RolesRead = "user-management.roles.read";
        public const string RolesManage = "user-management.roles.manage";
        public const string PermissionsRead = "user-management.permissions.read";
        public const string PermissionsManage = "user-management.permissions.manage";
    }

    /// <summary>
    /// Flattens all permissions into a single enumerable, used during seeding and policy registration.
    /// </summary>
    public static IReadOnlyCollection<string> All { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Finance.BudgetsRead,
        Finance.BudgetsManage,
        Finance.ExpensesRead,
        Finance.ExpensesManage,
        Finance.InvoicesRead,
        Finance.InvoicesManage,
        Finance.PayrollRead,
        Finance.PayrollManage,
        Finance.CompensationRead,
        Finance.CompensationManage,
        Finance.SummaryView,

        Hr.EmployeesRead,
        Hr.EmployeesManage,
        Hr.AttendanceRead,
        Hr.AttendanceManage,
        Hr.DepartmentsRead,
        Hr.DepartmentsManage,
        Hr.LeavesRead,
        Hr.LeavesManage,
        Hr.JobOpeningsRead,
        Hr.JobOpeningsManage,
        Hr.JobApplicationsRead,
        Hr.JobApplicationsManage,
        Hr.InterviewsRead,
        Hr.InterviewsManage,
        Hr.ResignationsRead,
        Hr.ResignationsManage,
        Hr.SummaryView,

        Inventory.ItemsRead,
        Inventory.ItemsManage,
        Inventory.WarehousesRead,
        Inventory.WarehousesManage,
        Inventory.StockRead,
        Inventory.StockManage,
        Inventory.SummaryView,

        Reporting.OverviewView,
        Reporting.FinanceView,
        Reporting.HrView,
        Reporting.InventoryView,
        Reporting.ExportPdf,
        Reporting.ExportExcel,

        UserManagement.UsersRead,
        UserManagement.UsersManage,
        UserManagement.RolesRead,
        UserManagement.RolesManage,
        UserManagement.PermissionsRead,
        UserManagement.PermissionsManage,
    };
}
