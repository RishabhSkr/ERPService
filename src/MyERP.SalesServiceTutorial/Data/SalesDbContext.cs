using Microsoft.EntityFrameworkCore;
using MyERP.SalesServiceTutorial.Models;

namespace MyERP.SalesServiceTutorial.Data;
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

        modelBuilder.Entity<Customer>(entity => {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Phone).HasMaxLength(20);
            entity.Property(c => c.Address).HasMaxLength(255);
            entity.Property(c => c.City).HasMaxLength(50);
            entity.Property(c => c.Country).HasMaxLength(50);
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(c => c.UpdatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<SalesOrder>(entity =>{
            entity.HasKey(s => s.Id);
            entity.Property(s => s.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(s => s.OrderDate).HasDefaultValueSql("GETDATE()");
            entity.Property(s => s.TotalAmount).HasDefaultValue(0).HasPrecision(18,2);
            entity.Property(s => s.OrderStatus).HasDefaultValue("Pending");
            entity.Property(s => s.PaymentStatus).HasDefaultValue("Unpaid");
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(s => s.UpdatedAt).HasDefaultValueSql("GETDATE()");

            // one to many relationship
            entity.HasOne(o => o.Customer)
                .WithMany(c => c.SalesOrders)
                .HasForeignKey(o => o.CustomerId);
        });

        modelBuilder.Entity<SalesOrderItem>(entity =>
        {
            entity.HasKey(si => si.Id);
            entity.Property(si => si.ProductName).IsRequired().HasMaxLength(100);
            entity.Property(si => si.ProductCode).IsRequired().HasMaxLength(50);
            entity.Property(si => si.Quantity).HasDefaultValue(0).HasPrecision(18,2);
            entity.Property(si => si.UnitPrice).HasDefaultValue(0).HasPrecision(18,2);
            entity.Property(si => si.TotalPrice).HasDefaultValue(0).HasPrecision(18,2);
            entity.Property(si => si.CreatedAt).HasDefaultValueSql("GETDATE()");


            // many to one relationship
           entity.HasOne(si => si.SalesOrder)
                .WithMany(o => o.Items)
                .HasForeignKey(si => si.SalesOrderId);
        });
    }
}   

