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

        // Work Orders & Routing
        public DbSet<Process> Processes { get; set; } = null!;
        public DbSet<WorkCenter> WorkCenters { get; set; } = null!;
        public DbSet<Equipment> Equipment { get; set; } = null!;
        public DbSet<EquipmentProcess> EquipmentProcesses { get; set; } = null!;
        public DbSet<ProcessRoute> ProcessRoutes { get; set; } = null!;
        public DbSet<ProcessRouteStep> ProcessRouteSteps { get; set; } = null!;
        public DbSet<ProcessRouteStepMaterial> ProcessRouteStepMaterials { get; set; } = null!;
        public DbSet<WorkOrder> WorkOrders { get; set; } = null!;
        public DbSet<WorkOrderExecution> WorkOrderExecutions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // BOM Configuration
            // ========================================
            modelBuilder.Entity<BOM>(entity =>
            {
                entity.HasKey(e => e.BOMId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => new { e.ProductId, e.Version }).IsUnique();
                entity.HasIndex(e => e.BomCode).IsUnique();
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
                entity.HasIndex(e => e.ProcessId);
                entity.HasOne(e => e.Process)
                      .WithMany()
                      .HasForeignKey(e => e.ProcessId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // PendingRequest Configuration
            // ========================================
            modelBuilder.Entity<PendingRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SalesOrderId).IsUnique();
                entity.HasIndex(e => e.Status);
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
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.SalesOrderId);
                entity.Property(e => e.QuantityPlanned).HasPrecision(18, 4);
                entity.Property(e => e.QuantityGood).HasPrecision(18, 4);
                entity.Property(e => e.QuantityScrap).HasPrecision(18, 4);
                entity.Ignore(e => e.QuantityProduced);
                entity.HasIndex(e => e.ReservationStatus);
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

            // ========================================
            // Process Configuration
            // ========================================
            modelBuilder.Entity<Process>(entity =>
            {
                entity.HasKey(e => e.ProcessId);
                entity.HasIndex(e => e.ProcessCode).IsUnique();
                entity.HasIndex(e => e.Category);
            });

            // ========================================
            // WorkCenter Configuration
            // ========================================
            modelBuilder.Entity<WorkCenter>(entity =>
            {
                entity.HasKey(e => e.WorkCenterId);
                entity.HasIndex(e => e.CenterCode).IsUnique();
                entity.HasMany(e => e.Equipment)
                      .WithOne(e => e.WorkCenter)
                      .HasForeignKey(e => e.WorkCenterId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.ProcessRoutes)
                      .WithOne(e => e.WorkCenter)
                      .HasForeignKey(e => e.WorkCenterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // Equipment Configuration
            // ========================================
            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => e.EquipmentId);
                entity.HasIndex(e => e.EquipmentCode).IsUnique();
                entity.Property(e => e.CostPerHour).HasPrecision(18, 2);
            });

            // ========================================
            // ProcessRoute Configuration
            // ========================================
            modelBuilder.Entity<ProcessRoute>(entity =>
            {
                entity.HasKey(e => e.ProcessRouteId);
                entity.HasIndex(e => e.RouteCode).IsUnique();
                entity.HasIndex(e => e.ProductId);
                entity.HasMany(e => e.Steps)
                      .WithOne(e => e.ProcessRoute)
                      .HasForeignKey(e => e.ProcessRouteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProcessRouteStep>(entity =>
            {
                entity.HasKey(e => e.ProcessRouteStepId);
                entity.HasMany(e => e.Materials)
                      .WithOne(e => e.ProcessRouteStep)
                      .HasForeignKey(e => e.ProcessRouteStepId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProcessRouteStepMaterial>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasPrecision(18, 4);
            });

            // ========================================
            // EquipmentProcess Configuration
            // ========================================
            modelBuilder.Entity<EquipmentProcess>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.EquipmentId, e.ProcessId }).IsUnique();
            });

            // ========================================
            // WorkOrder Configuration
            // ========================================
            modelBuilder.Entity<WorkOrder>(entity =>
            {
                entity.HasKey(e => e.WorkOrderId);
                entity.HasIndex(e => e.WorkOrderNumber).IsUnique();
                entity.HasIndex(e => e.ProductionOrderId);
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.QuantityPlanned).HasPrecision(18, 4);
                entity.Property(e => e.QuantityCompleted).HasPrecision(18, 4);
                entity.Property(e => e.QuantityScrap).HasPrecision(18, 4);
                entity.HasMany(e => e.Executions)
                      .WithOne(e => e.WorkOrder)
                      .HasForeignKey(e => e.WorkOrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // WorkOrderExecution Configuration
            // ========================================
            modelBuilder.Entity<WorkOrderExecution>(entity =>
            {
                entity.HasKey(e => e.ExecutionId);
                entity.HasIndex(e => e.WorkOrderId);
                entity.HasIndex(e => e.EquipmentId);
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.QuantityProduced).HasPrecision(18, 4);
                entity.Property(e => e.QuantityScrap).HasPrecision(18, 4);
            });
        }
    }
}
