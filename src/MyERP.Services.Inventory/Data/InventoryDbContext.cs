using Microsoft.EntityFrameworkCore;
using MyERP.Services.Inventory.Models;

namespace MyERP.Services.Inventory.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<StorageLocation> StorageLocations { get; set; }
        public DbSet<StorageLocationType> StorageLocationTypes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<RawMaterial> RawMaterials { get; set; }
        public DbSet<ProductInventory> ProductInventories { get; set; }
        public DbSet<RawMaterialInventory> RawMaterialInventories { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // 1. Category Config
            // ============================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CategoryCode).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.CategoryCode).IsUnique();
            });

            // ============================
            // 2. Unit Config
            // ============================
            modelBuilder.Entity<Unit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UnitCode).IsRequired().HasMaxLength(10);
                entity.HasIndex(e => e.UnitCode).IsUnique();
            });

            // ============================
            // 3. Warehouse Config
            // ============================
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WarehouseName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.WarehouseCode).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.WarehouseCode).IsUnique();
            });

            // ============================
            // 3b. StorageLocation Config
            // ============================
            modelBuilder.Entity<StorageLocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LocationCode).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.LocationCode).IsUnique();

                entity.HasOne(d => d.Warehouse)
                      .WithMany(p => p.StorageLocations)
                      .HasForeignKey(d => d.WarehouseId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(d => d.LocationType)
                      .WithMany(p => p.StorageLocations)
                      .HasForeignKey(d => d.LocationTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================
            // 3c. StorageLocationType Config
            // ============================
            modelBuilder.Entity<StorageLocationType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TypeCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.TypeCode).IsUnique();
            });

            // ============================
            // 4. Product Config
            // ============================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.MinStockLevel).HasPrecision(18, 2);
                entity.HasIndex(e => e.ProductCode).IsUnique();

                entity.HasOne(d => d.Category)
                      .WithMany(p => p.Products)
                      .HasForeignKey(d => d.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Unit)
                      .WithMany(p => p.Products)
                      .HasForeignKey(d => d.UnitId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================
            // 5. RawMaterial Config
            // ============================
            modelBuilder.Entity<RawMaterial>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MaterialCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MaterialName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Cost).HasPrecision(18, 2);
                entity.Property(e => e.MinStockLevel).HasPrecision(18, 2);
                entity.HasIndex(e => e.MaterialCode).IsUnique();

                entity.HasOne(d => d.Category)
                      .WithMany(p => p.RawMaterials)
                      .HasForeignKey(d => d.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Unit)
                      .WithMany(p => p.RawMaterials)
                      .HasForeignKey(d => d.UnitId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================
            // 6. ProductInventory Config
            // ============================
            modelBuilder.Entity<ProductInventory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CurrentStock).HasPrecision(18, 2);
                entity.Property(e => e.ReservedStock).HasPrecision(18, 2);
                entity.Ignore(e => e.AvailableStock); // Computed property

                entity.HasOne(d => d.Product)
                      .WithMany(p => p.ProductInventories)
                      .HasForeignKey(d => d.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.StorageLocation)
                      .WithMany(p => p.ProductInventories)
                      .HasForeignKey(d => d.StorageLocationId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint: One product per location per batch
                entity.HasIndex(e => new { e.ProductId, e.StorageLocationId, e.BatchNumber }).IsUnique();
            });

            // ============================
            // 7. RawMaterialInventory Config
            // ============================
            modelBuilder.Entity<RawMaterialInventory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CurrentStock).HasPrecision(18, 2);
                entity.Property(e => e.ReservedStock).HasPrecision(18, 2);
                entity.Ignore(e => e.AvailableStock); // Computed property

                entity.HasOne(d => d.RawMaterial)
                      .WithMany(p => p.RawMaterialInventories)
                      .HasForeignKey(d => d.RawMaterialId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.StorageLocation)
                      .WithMany(p => p.RawMaterialInventories)
                      .HasForeignKey(d => d.StorageLocationId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint
                entity.HasIndex(e => new { e.RawMaterialId, e.StorageLocationId, e.BatchNumber }).IsUnique();
            });

            // ============================
            // 8. StockMovement Config
            // ============================
            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ItemType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ItemCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ItemName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MovementType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Quantity).HasPrecision(18, 2);
                entity.Property(e => e.StockBefore).HasPrecision(18, 2);
                entity.Property(e => e.StockAfter).HasPrecision(18, 2);

                entity.HasOne(d => d.FromWarehouse)
                      .WithMany()
                      .HasForeignKey(d => d.FromWarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.FromLocation)
                      .WithMany()
                      .HasForeignKey(d => d.FromLocationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ToWarehouse)
                      .WithMany()
                      .HasForeignKey(d => d.ToWarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ToLocation)
                      .WithMany()
                      .HasForeignKey(d => d.ToLocationId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Index for faster queries
                entity.HasIndex(e => new { e.ItemType, e.ItemId });
                entity.HasIndex(e => e.CreatedAt);
            });

            // ============================
            // SEED DATA
            // ============================
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Fixed GUIDs for seed data (using valid hex characters only)
            var categoryBatteries = Guid.Parse("c1111111-1111-1111-1111-111111111111");
            var categoryChemicals = Guid.Parse("c2222222-2222-2222-2222-222222222222");
            var categoryMetals = Guid.Parse("c3333333-3333-3333-3333-333333333333");

            var unitPieces = Guid.Parse("a1111111-1111-1111-1111-111111111111");
            var unitKg = Guid.Parse("a2222222-2222-2222-2222-222222222222");
            var unitLiters = Guid.Parse("a3333333-3333-3333-3333-333333333333");

            var warehouseMain = Guid.Parse("b1111111-1111-1111-1111-111111111111");

            // 1. Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = categoryBatteries, CategoryName = "Batteries", CategoryCode = "CAT-BAT", Description = "All battery products", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = categoryChemicals, CategoryName = "Chemicals", CategoryCode = "CAT-CHM", Description = "Chemical raw materials", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = categoryMetals, CategoryName = "Metals", CategoryCode = "CAT-MTL", Description = "Metal raw materials", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // 2. Units
            modelBuilder.Entity<Unit>().HasData(
                new Unit { Id = unitPieces, UnitName = "Pieces", UnitCode = "PCS", Description = "Count of individual items", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Unit { Id = unitKg, UnitName = "Kilograms", UnitCode = "KG", Description = "Weight in kilograms", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Unit { Id = unitLiters, UnitName = "Liters", UnitCode = "LTR", Description = "Volume in liters", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // 3. Warehouse
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = warehouseMain, WarehouseName = "Main Warehouse", WarehouseCode = "WH-MAIN", Address = "123 Industrial Area", City = "Mumbai", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // 4. StorageLocationType
            var typeRm = Guid.Parse("d1111111-1111-1111-1111-111111111111");
            var typeFg = Guid.Parse("d2222222-2222-2222-2222-222222222222");
            var typeGen = Guid.Parse("d3333333-3333-3333-3333-333333333333");
            
            modelBuilder.Entity<StorageLocationType>().HasData(
                new StorageLocationType { Id = typeRm, TypeCode = "RM", TypeName = "Raw Materials", AllowRawMaterials = true, AllowProducts = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new StorageLocationType { Id = typeFg, TypeCode = "FG", TypeName = "Finished Goods", AllowRawMaterials = false, AllowProducts = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new StorageLocationType { Id = typeGen, TypeCode = "GEN", TypeName = "General Storage", AllowRawMaterials = true, AllowProducts = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
