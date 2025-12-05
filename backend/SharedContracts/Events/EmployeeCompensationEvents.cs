using System;
using System.Collections.Generic;

namespace SharedContracts.Events
{
    public record EmployeeCompensationProvisionedEvent(
        Guid EmployeeId,
        string FullName,
        string WorkEmail,
        string? PhoneNumber,
        string Position,
        Guid DepartmentId,
        string DepartmentName,
        decimal BaseSalary,
        string Currency,
        DateTime EffectiveFrom);

    public record EmployeeCompensationBonusesIssuedEvent(
        Guid EmployeeId,
        string EmployeeName,
        IReadOnlyCollection<EmployeeCompensationBonusItem> Bonuses,
        DateTime IssuedAt,
        string? IssuedBy);

    public record EmployeeCompensationBonusItem(
        string BonusType,
        decimal Amount,
        DateTime AwardedOn,
        string? Period,
        string? Reference,
        string? AwardedBy,
        string? Notes,
        string? SourceSystem);

    public record EmployeeCompensationDeductionsIssuedEvent(
        Guid EmployeeId,
        string EmployeeName,
        IReadOnlyCollection<EmployeeCompensationDeductionItem> Deductions,
        DateTime IssuedAt,
        string? IssuedBy);

    public record EmployeeCompensationDeductionItem(
        string DeductionType,
        decimal Amount,
        DateTime AppliedOn,
        string? Period,
        string? Reference,
        string? AppliedBy,
        string? Notes,
        string? SourceSystem,
        bool IsRecurring);
}
