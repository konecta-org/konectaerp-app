using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<StockLevel> StockLevels => Set<StockLevel>();
        public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InventoryItem>()
                .HasIndex(item => item.Sku)
                .IsUnique();

            modelBuilder.Entity<InventoryItem>()
                .Property(item => item.StandardCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InventoryItem>()
                .Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Warehouse>()
                .HasIndex(warehouse => warehouse.Code)
                .IsUnique();

            modelBuilder.Entity<StockLevel>()
                .HasKey(level => new { level.InventoryItemId, level.WarehouseId });

            modelBuilder.Entity<StockLevel>()
                .Property(level => level.QuantityOnHand)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StockLevel>()
                .Property(level => level.QuantityReserved)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StockLevel>()
                .Property(level => level.ReorderQuantity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StockLevel>()
                .HasOne(level => level.InventoryItem)
                .WithMany(item => item.StockLevels!)
                .HasForeignKey(level => level.InventoryItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockLevel>()
                .HasOne(level => level.Warehouse)
                .WithMany(warehouse => warehouse.StockLevels!)
                .HasForeignKey(level => level.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockTransaction>()
                .Property(transaction => transaction.Quantity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(transaction => transaction.InventoryItem)
                .WithMany(item => item.Transactions!)
                .HasForeignKey(transaction => transaction.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(transaction => transaction.Warehouse)
                .WithMany(warehouse => warehouse.Transactions!)
                .HasForeignKey(transaction => transaction.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
