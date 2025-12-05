using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SharedContracts.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(requirement.Permission))
        {
            return Task.CompletedTask;
        }

        var hasPermission = context.User.Claims.Any(claim =>
            string.Equals(claim.Type, PermissionConstants.ClaimType, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(claim.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
