using System.ComponentModel.DataAnnotations;

namespace InventoryService.Dtos
{
    public record WarehouseResponseDto(
        int Id,
        string Code,
        string Name,
        string? Location,
        string? ContactEmail,
        string Status,
        decimal TotalOnHand,
        decimal TotalReserved,
        decimal TotalAvailable);

    public class WarehouseUpsertDto
    {
        [Required]
        [MaxLength(32)]
        public string Code { get; set; } = default!;

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = default!;

        [MaxLength(256)]
        public string? Location { get; set; }

        [EmailAddress]
        [MaxLength(256)]
        public string? ContactEmail { get; set; }

        [MaxLength(32)]
        public string Status { get; set; } = "Active";
    }
}
