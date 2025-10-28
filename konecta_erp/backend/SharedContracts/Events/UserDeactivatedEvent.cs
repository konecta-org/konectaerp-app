using System;

namespace SharedContracts.Events
{
    public record UserDeactivatedEvent(
        string UserId,
        Guid EmployeeId,
        DateTime DeactivatedAt,
        string? Reason);
}

