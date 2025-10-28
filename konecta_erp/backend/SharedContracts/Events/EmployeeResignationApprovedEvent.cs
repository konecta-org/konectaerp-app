using System;

namespace SharedContracts.Events
{
    public record EmployeeResignationApprovedEvent(
        Guid ResignationRequestId,
        Guid EmployeeId,
        Guid? UserId,
        DateTime EffectiveDate,
        string? Reason,
        DateTime ApprovedAt);
}
