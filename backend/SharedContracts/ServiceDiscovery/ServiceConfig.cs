using System.Diagnostics.CodeAnalysis;

namespace SharedContracts.ServiceDiscovery;

/// Configuration used to register a service instance with Consul.
public sealed class ServiceConfig
{
    public string ServiceName { get; set; } = string.Empty;

 
    public string Host { get; set; } = "localhost";

    public int Port { get; set; }

    
    public string Scheme { get; set; } = "http";

    /// <summary>
    /// Optional hostname or IP published to Consul; defaults to <see cref="Host"/> when unspecified.
    /// </summary>
    public string? PublicHost { get; set; }

  
    [StringSyntax(StringSyntaxAttribute.Uri)]
    public string HealthCheckPath { get; set; } = "/system/health";

   
    public string[] Tags { get; set; } = Array.Empty<string>();

    public bool RegisterWithConsul { get; set; } = true;

    public bool FailFast { get; set; } = false;
}
