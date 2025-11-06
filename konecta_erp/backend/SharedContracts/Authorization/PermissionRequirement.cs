using Microsoft.AspNetCore.Authorization;

namespace SharedContracts.Authorization;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
