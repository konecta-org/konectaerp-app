using System.ComponentModel.DataAnnotations;

namespace InventoryService.Dtos
{
    public class WarehouseResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? ContactEmail { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalOnHand { get; set; }
        public decimal TotalReserved { get; set; }
        public decimal TotalAvailable { get; set; }
    }

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
