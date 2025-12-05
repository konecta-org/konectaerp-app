using System.ComponentModel.DataAnnotations;

namespace HrService.Dtos;

public class FireEmployeeRequestDto
{
    [MaxLength(2000)]
    public string? Reason { get; set; }

    public bool? EligibleForRehire { get; set; }
}
