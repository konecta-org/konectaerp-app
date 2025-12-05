using System.Net.Http.Json;
using System.Text;
using AuthenticationService.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedContracts.Configuration;

namespace AuthenticationService.Services;

public sealed class UserManagementClient : IUserManagementClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserManagementClient> _logger;
    private readonly ServiceAuthOptions _options;

    public UserManagementClient(HttpClient httpClient, IOptions<ServiceAuthOptions> options, ILogger<UserManagementClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<AuthorizationProfileDto?> GetAuthorizationProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SharedSecret))
        {
            _logger.LogWarning("ServiceAuth.SharedSecret is not configured; unable to fetch authorization profile.");
            return null;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}/authorizations");
        request.Headers.Add("X-Service-Token", _options.SharedSecret);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to fetch authorization profile for user {UserId}: {Status} {Body}", userId, response.StatusCode, body);
            return null;
        }

        var profile = await response.Content.ReadFromJsonAsync<AuthorizationProfileDto>(cancellationToken: cancellationToken);
        return profile;
    }
}
