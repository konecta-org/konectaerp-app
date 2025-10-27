using System;

namespace SharedContracts.Events
{
    public record EmployeeCreatedEvent(
        Guid EmployeeId,
        string FullName,
        string WorkEmail,
        string PersonalEmail,
        string Position,
        Guid DepartmentId,
        string DepartmentName,
        DateTime HireDate);
}
