using System;

namespace SharedContracts.Events;

public record UserTerminatedEvent(
    string UserId,
    Guid EmployeeId,
    DateTime TerminatedAt,
    string? Reason);
