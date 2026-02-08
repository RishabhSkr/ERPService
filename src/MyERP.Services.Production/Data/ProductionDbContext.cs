/*
 * ProductionDbContext - Database Context
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: Just add DbSets, let EF figure out everything
 * ✅ INDUSTRY:
 *    1. Configure relationships explicitly in OnModelCreating
 *    2. Set up indexes for frequently queried columns
 *    3. Configure cascade delete behavior
 *    4. Add unique constraints for business keys
 */

using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Data
{
    public class ProductionDbContext : DbContext
    {
        public ProductionDbContext(DbContextOptions<ProductionDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<BOM> BOMs { get; set; } = null!;
        public DbSet<BOMLine> BOMLines { get; set; } = null!;
        public DbSet<PendingRequest> PendingRequests { get; set; } = null!;
        public DbSet<PendingRequestItem> PendingRequestItems { get; set; } = null!;
        public DbSet<ProductionOrder> ProductionOrders { get; set; } = null!;
        public DbSet<MaterialRequirement> MaterialRequirements { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // BOM Configuration
            // ========================================
            modelBuilder.Entity<BOM>(entity =>
            {
                entity.HasKey(e => e.BOMId);
                
                // Index on ProductId for fast lookup
                entity.HasIndex(e => e.ProductId);
                
                // Unique constraint: only one active BOM per product per version
                entity.HasIndex(e => new { e.ProductId, e.Version }).IsUnique();
                
                // BOMCode should be unique
                entity.HasIndex(e => e.BOMCode).IsUnique();
                
                // One BOM has many Lines
                entity.HasMany(e => e.Lines)
                      .WithOne(e => e.BOM)
                      .HasForeignKey(e => e.BOMId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BOMLine>(entity =>
            {
                entity.HasKey(e => e.BOMLineId);
                entity.Property(e => e.Quantity).HasPrecision(18, 4);
                entity.Property(e => e.ScrapPercentage).HasPrecision(5, 2);
            });

            // ========================================
            // PendingRequest Configuration
            // ========================================
            modelBuilder.Entity<PendingRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Unique: one pending request per sales order
                entity.HasIndex(e => e.SalesOrderId).IsUnique();
                
                // Index for filtering by status
                entity.HasIndex(e => e.Status);
                
                // Idempotency index
                entity.HasIndex(e => e.EventId).IsUnique();
                
                entity.HasMany(e => e.Items)
                      .WithOne(e => e.PendingRequest)
                      .HasForeignKey(e => e.PendingRequestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PendingRequestItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasPrecision(18, 4);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
            });

            // ========================================
            // ProductionOrder Configuration
            // ========================================
            modelBuilder.Entity<ProductionOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Unique order number
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                
                // Index for filtering by status
                entity.HasIndex(e => e.Status);
                
                // Index for Sales Order lookup
                entity.HasIndex(e => e.SalesOrderId);
                
                // Precision for quantities
                entity.Property(e => e.QuantityPlanned).HasPrecision(18, 4);
                entity.Property(e => e.QuantityGood).HasPrecision(18, 4);
                entity.Property(e => e.QuantityScrap).HasPrecision(18, 4);
                
                // Ignore computed property
                entity.Ignore(e => e.QuantityProduced);
                
                entity.HasMany(e => e.MaterialRequirements)
                      .WithOne(e => e.ProductionOrder)
                      .HasForeignKey(e => e.ProductionOrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MaterialRequirement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuantityRequired).HasPrecision(18, 4);
                entity.Property(e => e.QuantityReserved).HasPrecision(18, 4);
                entity.Property(e => e.QuantityConsumed).HasPrecision(18, 4);
            });
        }
    }
}
