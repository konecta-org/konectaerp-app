namespace AuthenticationService.Dtos;

public sealed record AuthorizationProfileDto(
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
