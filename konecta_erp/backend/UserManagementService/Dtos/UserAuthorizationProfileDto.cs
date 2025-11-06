namespace UserManagementService.Dtos;

public record UserAuthorizationProfileDto(
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
