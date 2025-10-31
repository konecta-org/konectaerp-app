using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SharedContracts.ServiceDiscovery;

public static class ServiceDiscoveryExtensions
{
    /// Adds Consul registration services and exposes ServiceConfig for downstream consumers.
    public static IServiceCollection AddConsulServiceDiscovery(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceConfig>(configuration.GetSection("ServiceConfig"));

        services.AddSingleton<IConsulClient>(_ =>
        {
            var consulAddress = configuration["Consul:Host"];
            var consulToken = configuration["Consul:Token"];

            return new ConsulClient(options =>
            {
                if (!string.IsNullOrWhiteSpace(consulAddress))
                {
                    options.Address = new Uri(consulAddress);
                }

                if (!string.IsNullOrWhiteSpace(consulToken))
                {
                    options.Token = consulToken;
                }
            });
        });

        services.AddHostedService<ConsulRegistrationHostedService>();

        services.AddSingleton<ServiceConfig>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ServiceConfig>>().Value;
            if (options.Port <= 0)
            {
                throw new InvalidOperationException("ServiceConfig.Port must be a positive number.");
            }

            return options;
        });

        return services;
    }
}
