using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace SharedContracts.Authorization;

public static class ServiceCollectionAuthorizationExtensions
{
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddOptions<AuthorizationOptions>()
            .Configure(options =>
            {
                foreach (var permission in PermissionConstants.All)
                {
                    if (options.GetPolicy(permission) == null)
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
