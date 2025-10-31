using System;

namespace SharedContracts.Events;

public record EmployeeTerminatedEvent(
    Guid EmployeeId,
    Guid? UserId,
    DateTime TerminatedAt,
    string? Reason,
    bool EligibleForRehire);
