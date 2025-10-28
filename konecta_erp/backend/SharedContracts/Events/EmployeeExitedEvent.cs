using System;

namespace SharedContracts.Events
{
    public record EmployeeExitedEvent(
        Guid EmployeeId,
        string? UserId,
        DateTime ExitDate,
        string? Reason,
        bool? EligibleForRehire,
        string ExitStatus);
}

