namespace InventoryService.Models
{
    public static class StockTransactionTypes
    {
        public const string Receipt = "Receipt";
        public const string Issue = "Issue";
        public const string Adjustment = "Adjustment";
        public const string TransferIn = "TransferIn";
        public const string TransferOut = "TransferOut";

        public static readonly IReadOnlyCollection<string> All = new[]
        {
            Receipt,
            Issue,
            Adjustment,
            TransferIn,
            TransferOut
        };

        public static bool IsValid(string? value) =>
            !string.IsNullOrWhiteSpace(value) && All.Contains(value);
    }
}
