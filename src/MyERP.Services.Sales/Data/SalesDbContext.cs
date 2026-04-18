using Microsoft.EntityFrameworkCore;
using MyERP.Services.Sales.Models;

namespace MyERP.Services.Sales.Data
{
    public class SalesDbContext : DbContext
    {
        public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.CustomerCode).IsUnique();
                entity.Property(c => c.CustomerCode).IsRequired().HasMaxLength(50);
                entity.Property(c => c.CustomerName).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Email).HasMaxLength(200);
                entity.Property(c => c.Phone).HasMaxLength(50);
            });

            // SalesOrder
            modelBuilder.Entity<SalesOrder>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.HasIndex(o => o.OrderNumber).IsUnique();
                entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(o => o.OrderStatus).IsRequired().HasMaxLength(50);
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

                entity.HasOne(o => o.Customer)
                    .WithMany(c => c.SalesOrders)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SalesOrderItem
            modelBuilder.Entity<SalesOrderItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.ProductCode).IsRequired().HasMaxLength(50);
                entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
                entity.Property(i => i.TotalPrice).HasPrecision(18, 2);
                entity.Property(i => i.QuantityProduced).HasPrecision(18, 2);
                entity.Property(i => i.QuantityDispatched).HasPrecision(18, 2);
                entity.HasOne(i => i.SalesOrder)
                    .WithMany(o => o.Items)
                    .HasForeignKey(i => i.SalesOrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var customerId1 = Guid.Parse("d1111111-1111-1111-1111-111111111111");
            var customerId2 = Guid.Parse("d2222222-2222-2222-2222-222222222222");

            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    Id = customerId1,
                    CustomerCode = "CUST-001",
                    CustomerName = "ABC Manufacturing Ltd",
                    Email = "contact@abc-mfg.com",
                    Phone = "+91-9876543210",
                    Address = "Industrial Area, Plot 45",
                    City = "Mumbai",
                    Country = "India",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Customer
                {
                    Id = customerId2,
                    CustomerCode = "CUST-002",
                    CustomerName = "XYZ Industries",
                    Email = "info@xyz-ind.com",
                    Phone = "+91-9876543211",
                    Address = "Tech Park, Building B",
                    City = "Bangalore",
                    Country = "India",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
