using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedContracts.Authorization;
using UserManagementService.Data;
using UserManagementService.Models;

namespace UserManagementService.BackgroundServices;

/// <summary>
/// Bootstraps default roles and permissions so that downstream services can enforce authorization consistently.
/// </summary>
public class AuthorizationSeederHostedService : IHostedService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> RolePermissionMap =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [RoleConstants.SystemAdmin] = PermissionConstants.All.ToArray(),

            [RoleConstants.HrAdmin] = new[]
            {
                PermissionConstants.Hr.EmployeesRead,
                PermissionConstants.Hr.EmployeesManage,
                PermissionConstants.Hr.AttendanceRead,
                PermissionConstants.Hr.AttendanceManage,
                PermissionConstants.Hr.DepartmentsRead,
                PermissionConstants.Hr.DepartmentsManage,
                PermissionConstants.Hr.LeavesRead,
                PermissionConstants.Hr.LeavesManage,
                PermissionConstants.Hr.JobOpeningsRead,
                PermissionConstants.Hr.JobOpeningsManage,
                PermissionConstants.Hr.JobApplicationsRead,
                PermissionConstants.Hr.JobApplicationsManage,
                PermissionConstants.Hr.InterviewsRead,
                PermissionConstants.Hr.InterviewsManage,
                PermissionConstants.Hr.ResignationsRead,
                PermissionConstants.Hr.ResignationsManage,
                PermissionConstants.Hr.SummaryView,
                PermissionConstants.Reporting.OverviewView,
                PermissionConstants.Reporting.HrView,
                PermissionConstants.Reporting.ExportPdf,
                PermissionConstants.Reporting.ExportExcel
            },

            [RoleConstants.HrStaff] = new[]
            {
                PermissionConstants.Hr.EmployeesRead,
                PermissionConstants.Hr.AttendanceRead,
                PermissionConstants.Hr.AttendanceManage,
                PermissionConstants.Hr.LeavesRead,
                PermissionConstants.Hr.LeavesManage,
                PermissionConstants.Hr.JobApplicationsRead,
                PermissionConstants.Hr.JobApplicationsManage,
                PermissionConstants.Hr.InterviewsRead,
                PermissionConstants.Hr.InterviewsManage,
                PermissionConstants.Hr.SummaryView
            },

            [RoleConstants.FinanceManager] = new[]
            {
                PermissionConstants.Finance.BudgetsRead,
                PermissionConstants.Finance.BudgetsManage,
                PermissionConstants.Finance.ExpensesRead,
                PermissionConstants.Finance.ExpensesManage,
                PermissionConstants.Finance.InvoicesRead,
                PermissionConstants.Finance.InvoicesManage,
                PermissionConstants.Finance.PayrollRead,
                PermissionConstants.Finance.PayrollManage,
                PermissionConstants.Finance.CompensationRead,
                PermissionConstants.Finance.CompensationManage,
                PermissionConstants.Finance.SummaryView,
                PermissionConstants.Reporting.OverviewView,
                PermissionConstants.Reporting.FinanceView,
                PermissionConstants.Reporting.ExportPdf,
                PermissionConstants.Reporting.ExportExcel
            },

            [RoleConstants.FinanceStaff] = new[]
            {
                PermissionConstants.Finance.BudgetsRead,
                PermissionConstants.Finance.ExpensesRead,
                PermissionConstants.Finance.ExpensesManage,
                PermissionConstants.Finance.InvoicesRead,
                PermissionConstants.Finance.InvoicesManage,
                PermissionConstants.Finance.CompensationRead,
                PermissionConstants.Finance.SummaryView,
                PermissionConstants.Reporting.OverviewView,
                PermissionConstants.Reporting.FinanceView
            },

            [RoleConstants.DepartmentManager] = new[]
            {
                PermissionConstants.Hr.EmployeesRead,
                PermissionConstants.Hr.AttendanceRead,
                PermissionConstants.Hr.AttendanceManage,
                PermissionConstants.Hr.LeavesRead,
                PermissionConstants.Hr.LeavesManage,
                PermissionConstants.Hr.SummaryView,
                PermissionConstants.Reporting.OverviewView,
                PermissionConstants.Reporting.HrView
            },

            [RoleConstants.Employee] = new[]
            {
                PermissionConstants.Hr.AttendanceRead,
                PermissionConstants.Hr.LeavesRead,
                PermissionConstants.Finance.CompensationRead,
                PermissionConstants.Reporting.OverviewView
            }
        };

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthorizationSeederHostedService> _logger;

    public AuthorizationSeederHostedService(IServiceProvider serviceProvider, ILogger<AuthorizationSeederHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await EnsurePermissionsAsync(context, cancellationToken);
        await EnsureRolesAsync(context, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsurePermissionsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.Permissions.AsNoTracking()
            .Select(permission => permission.Name)
            .ToListAsync(cancellationToken);

        var newPermissions = PermissionConstants.All
            .Except(existing, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (newPermissions.Count == 0)
        {
            return;
        }

        foreach (var permission in newPermissions)
        {
            context.Permissions.Add(new Permission
            {
                Name = permission,
                Category = permission.Split('.')[0],
                Description = $"Auto-generated permission for {permission}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _logger.LogInformation("Seeded {PermissionCount} new permissions", newPermissions.Count);
    }

    private async Task EnsureRolesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var roles = await context.Roles
            .Include(role => role.Permissions!)
            .ToListAsync(cancellationToken);

        foreach (var roleName in RoleConstants.All)
        {
            var role = roles.FirstOrDefault(r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                role = new Role
                {
                    Name = roleName,
                    Description = $"{roleName} role",
                    IsSystemDefault = roleName is RoleConstants.SystemAdmin or RoleConstants.HrAdmin or RoleConstants.FinanceManager,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Permissions = new List<RolePermission>()
                };
                context.Roles.Add(role);
                roles.Add(role);
                _logger.LogInformation("Created role '{RoleName}'", roleName);
            }

            if (!RolePermissionMap.TryGetValue(role.Name, out var desiredPermissions))
            {
                continue;
            }

            role.Permissions ??= new List<RolePermission>();
            var currentPermissionIds = role.Permissions.Select(rp => rp.PermissionId).ToHashSet();

            var permissionLookup = await context.Permissions
                .Where(p => desiredPermissions.Contains(p.Name))
                .ToDictionaryAsync(p => p.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

            foreach (var permissionName in desiredPermissions)
            {
                if (!permissionLookup.TryGetValue(permissionName, out var permission))
                {
                    continue;
                }

                if (!currentPermissionIds.Contains(permission.Id))
                {
                    role.Permissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }

            // Remove permissions that are no longer mapped
            var desiredIds = desiredPermissions
                .Select(name => permissionLookup.TryGetValue(name, out var permission) ? permission.Id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            var toRemove = role.Permissions.Where(rp => !desiredIds.Contains(rp.PermissionId)).ToList();
            foreach (var removal in toRemove)
            {
                role.Permissions.Remove(removal);
            }
        }
    }
}
