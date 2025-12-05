using System.ComponentModel.DataAnnotations;

namespace InventoryService.Models
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string Code { get; set; } = default!;

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = default!;

        [MaxLength(256)]
        public string? Location { get; set; }

        [MaxLength(256)]
        public string? ContactEmail { get; set; }

        [MaxLength(32)]
        public string Status { get; set; } = "Active";

        public ICollection<StockLevel>? StockLevels { get; set; }

        public ICollection<StockTransaction>? Transactions { get; set; }
    }
}
