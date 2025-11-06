using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace SharedContracts.Authorization;

public static class ServiceCollectionAuthorizationExtensions
{
    /// <summary>
    /// Registers the permission-based authorization handler and configures policies for each declared permission.
    /// Call this once in service startup after <c>AddAuthentication</c>.
    /// </summary>
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddOptions<AuthorizationOptions>()
            .Configure(options =>
            {
                foreach (var permission in PermissionConstants.All)
                {
                    if (!options.PolicyNames.Contains(permission))
                    {
                        options.AddPolicy(permission, builder =>
                        {
                            builder.RequireAuthenticatedUser();
                            builder.AddRequirements(new PermissionRequirement(permission));
                        });
                    }
                }
            });

        return services;
    }
}
