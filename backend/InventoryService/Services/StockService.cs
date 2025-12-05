using InventoryService.Data;
using InventoryService.Dtos;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services
{
    public class StockService : IStockService
    {
        private readonly AppDbContext _context;

        public StockService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StockTransaction> AdjustStockAsync(StockAdjustmentRequestDto request, CancellationToken cancellationToken = default)
        {
            ValidateTransactionType(request.TransactionType);

            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == request.InventoryItemId, cancellationToken);
            if (item == null)
            {
                throw new InvalidOperationException($"Inventory item {request.InventoryItemId} not found.");
            }

            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);
            if (warehouse == null)
            {
                throw new InvalidOperationException($"Warehouse {request.WarehouseId} not found.");
            }

            var stockLevel = await _context.StockLevels.FirstOrDefaultAsync(
                level => level.InventoryItemId == request.InventoryItemId && level.WarehouseId == request.WarehouseId,
                cancellationToken);

            if (stockLevel == null)
            {
                stockLevel = new StockLevel
                {
                    InventoryItemId = request.InventoryItemId,
                    WarehouseId = request.WarehouseId,
                    QuantityOnHand = 0,
                    QuantityReserved = 0,
                    ReorderQuantity = 0
                };
                await _context.StockLevels.AddAsync(stockLevel, cancellationToken);
            }

            var delta = CalculateDelta(request.TransactionType, request.Quantity, request.Increase);

            if (stockLevel.QuantityOnHand + delta < 0)
            {
                throw new InvalidOperationException("Insufficient inventory to perform this operation.");
            }

            stockLevel.QuantityOnHand = Round(stockLevel.QuantityOnHand + delta);

            var transaction = new StockTransaction
            {
                InventoryItemId = request.InventoryItemId,
                WarehouseId = request.WarehouseId,
                Quantity = Round(Math.Abs(delta)),
                TransactionType = request.TransactionType,
                ReferenceNumber = request.ReferenceNumber,
                PerformedBy = request.PerformedBy,
                Notes = request.Notes,
                OccurredAtUtc = DateTime.UtcNow
            };

            await _context.StockTransactions.AddAsync(transaction, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _context.Entry(transaction).Reference(t => t.InventoryItem).LoadAsync(cancellationToken);
            await _context.Entry(transaction).Reference(t => t.Warehouse).LoadAsync(cancellationToken);

            return transaction;
        }

        public async Task<IEnumerable<StockTransaction>> TransferStockAsync(StockTransferRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request.FromWarehouseId == request.ToWarehouseId)
            {
                throw new InvalidOperationException("Source and destination warehouse must be different.");
            }

            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == request.InventoryItemId, cancellationToken);
            if (item == null)
            {
                throw new InvalidOperationException($"Inventory item {request.InventoryItemId} not found.");
            }

            var fromWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.FromWarehouseId, cancellationToken);
            if (fromWarehouse == null)
            {
                throw new InvalidOperationException($"Source warehouse {request.FromWarehouseId} not found.");
            }

            var toWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.ToWarehouseId, cancellationToken);
            if (toWarehouse == null)
            {
                throw new InvalidOperationException($"Destination warehouse {request.ToWarehouseId} not found.");
            }

            var fromLevel = await _context.StockLevels.FirstOrDefaultAsync(
                level => level.InventoryItemId == request.InventoryItemId && level.WarehouseId == request.FromWarehouseId,
                cancellationToken);

            if (fromLevel == null || fromLevel.QuantityOnHand < request.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock available in the source warehouse.");
            }

            var toLevel = await _context.StockLevels.FirstOrDefaultAsync(
                level => level.InventoryItemId == request.InventoryItemId && level.WarehouseId == request.ToWarehouseId,
                cancellationToken);

            if (toLevel == null)
            {
                toLevel = new StockLevel
                {
                    InventoryItemId = request.InventoryItemId,
                    WarehouseId = request.ToWarehouseId,
                    QuantityOnHand = 0,
                    QuantityReserved = 0,
                    ReorderQuantity = 0
                };
                await _context.StockLevels.AddAsync(toLevel, cancellationToken);
            }

            fromLevel.QuantityOnHand = Round(fromLevel.QuantityOnHand - request.Quantity);
            toLevel.QuantityOnHand = Round(toLevel.QuantityOnHand + request.Quantity);

            var transferOut = new StockTransaction
            {
                InventoryItemId = request.InventoryItemId,
                WarehouseId = request.FromWarehouseId,
                Quantity = Round(request.Quantity),
                TransactionType = StockTransactionTypes.TransferOut,
                ReferenceNumber = request.ReferenceNumber,
                PerformedBy = request.PerformedBy,
                Notes = request.Notes,
                OccurredAtUtc = DateTime.UtcNow
            };

            var transferIn = new StockTransaction
            {
                InventoryItemId = request.InventoryItemId,
                WarehouseId = request.ToWarehouseId,
                Quantity = Round(request.Quantity),
                TransactionType = StockTransactionTypes.TransferIn,
                ReferenceNumber = request.ReferenceNumber,
                PerformedBy = request.PerformedBy,
                Notes = request.Notes,
                OccurredAtUtc = DateTime.UtcNow
            };

            await _context.StockTransactions.AddRangeAsync(new[] { transferOut, transferIn }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _context.Entry(transferOut).Reference(t => t.InventoryItem).LoadAsync(cancellationToken);
            await _context.Entry(transferOut).Reference(t => t.Warehouse).LoadAsync(cancellationToken);
            await _context.Entry(transferIn).Reference(t => t.InventoryItem).LoadAsync(cancellationToken);
            await _context.Entry(transferIn).Reference(t => t.Warehouse).LoadAsync(cancellationToken);

            return new[] { transferOut, transferIn };
        }

        private static void ValidateTransactionType(string transactionType)
        {
            if (!StockTransactionTypes.IsValid(transactionType))
            {
                throw new InvalidOperationException($"Unsupported transaction type '{transactionType}'.");
            }
        }

        private static decimal CalculateDelta(string transactionType, decimal quantity, bool increaseFlag)
        {
            quantity = Round(quantity);

            return transactionType switch
            {
                StockTransactionTypes.Receipt => quantity,
                StockTransactionTypes.TransferIn => quantity,
                StockTransactionTypes.Issue => -quantity,
                StockTransactionTypes.TransferOut => -quantity,
                StockTransactionTypes.Adjustment => increaseFlag ? quantity : -quantity,
                _ => throw new InvalidOperationException($"Unsupported transaction type '{transactionType}'.")
            };
        }

        private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
