using System;

namespace SharedContracts.Events
{
    public record UserResignedEvent(
        string UserId,
        Guid EmployeeId,
        Guid ResignationRequestId,
        DateTime ResignedAt);
}
