using System;
using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SharedContracts.ServiceDiscovery;

/// Registers the current service instance with Consul on startup and removes it on shutdown.
public sealed class ConsulRegistrationHostedService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<ConsulRegistrationHostedService> _logger;
    private readonly ServiceConfig _serviceConfig;
    private AgentServiceRegistration? _registration;

    public ConsulRegistrationHostedService(
        IConsulClient consulClient,
        IHostApplicationLifetime applicationLifetime,
        IOptions<ServiceConfig> serviceConfigOptions,
        ILogger<ConsulRegistrationHostedService> logger)
    {
        _consulClient = consulClient;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
        _serviceConfig = serviceConfigOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_serviceConfig.RegisterWithConsul)
        {
            _logger.LogInformation("Consul registration disabled for {ServiceName}.", _serviceConfig.ServiceName ?? "service");
            return;
        }

        if (string.IsNullOrWhiteSpace(_serviceConfig.ServiceName))
        {
            _logger.LogWarning("Consul registration skipped because ServiceConfig.ServiceName is not set.");
            return;
        }

        var registrationId = $"{_serviceConfig.ServiceName}-{Guid.NewGuid():N}";
        var healthCheckUri = BuildHealthCheckUri();

        _registration = new AgentServiceRegistration
        {
            ID = registrationId,
            Name = _serviceConfig.ServiceName,
            Address = _serviceConfig.Host,
            Port = _serviceConfig.Port,
            Tags = _serviceConfig.Tags,
            Check = new AgentServiceCheck
            {
                HTTP = healthCheckUri,
                Interval = TimeSpan.FromSeconds(15),
                Timeout = TimeSpan.FromSeconds(5),
                TLSSkipVerify = string.Equals(_serviceConfig.Scheme, "https", StringComparison.OrdinalIgnoreCase),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            }
        };

        _logger.LogInformation("Registering service {ServiceName} in Consul at {HealthCheckUri}", _serviceConfig.ServiceName, healthCheckUri);

        try
        {
            await _consulClient.Agent.ServiceRegister(_registration, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register {ServiceName} with Consul.", _serviceConfig.ServiceName);
            _registration = null;

            if (_serviceConfig.FailFast)
            {
                throw;
            }

            _logger.LogWarning("Continuing without Consul registration because fail-fast is disabled.");
            return;
        }

        _applicationLifetime.ApplicationStopping.Register(async () => await DeregisterAsync());
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DeregisterAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task DeregisterAsync(CancellationToken cancellationToken = default)
    {
        if (_registration == null)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Deregistering service {ServiceId} from Consul.", _registration.ID);
            await _consulClient.Agent.ServiceDeregister(_registration.ID, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deregister service {ServiceId} from Consul.", _registration.ID);
        }
    }

    private string BuildHealthCheckUri()
    {
        var healthPath = _serviceConfig.HealthCheckPath?.Trim() ?? "/system/health";
        if (!healthPath.StartsWith("/"))
        {
            healthPath = "/" + healthPath;
        }

        var scheme = string.IsNullOrWhiteSpace(_serviceConfig.Scheme)
            ? "http"
            : _serviceConfig.Scheme.ToLowerInvariant();

        return $"{scheme}://{_serviceConfig.Host}:{_serviceConfig.Port}{healthPath}";
    }
}
