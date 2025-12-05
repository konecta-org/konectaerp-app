namespace AuthenticationService.Services;

public sealed record TokenResult(string AccessToken, DateTime ExpiresAtUtc, string KeyId);
