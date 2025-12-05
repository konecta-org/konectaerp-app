using System;
using System.Collections.Generic;

namespace SharedContracts.Events
{
    public record UserProvisionedEvent(
        string UserId,
        Guid EmployeeId,
        string WorkEmail,
        string FullName,
        IReadOnlyCollection<string> Roles,
        DateTime ProvisionedAt);
}
