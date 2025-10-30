using System;

namespace HrService.Dtos
{
    public record HrSummaryDto(
        int TotalEmployees,
        int ActiveEmployees,
        int Departments,
        int PendingResignations
    );
}
