using Microsoft.EntityFrameworkCore;
using MyERP.Services.Identity.Models;

namespace MyERP.Services.Identity.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // 1. RolePermission Config
            // ============================
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.RolePermissionId); // PK is Guid
                entity.Property(e => e.PermissionName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ApiEndpoint).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HttpMethod).IsRequired().HasMaxLength(10);

                // Relationships
                entity.HasOne(d => d.Role)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(d => d.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Module)
                      .WithMany(p => p.Permissions)
                      .HasForeignKey(d => d.ModuleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================
            // 2. User Config
            // ============================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id); 
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(d => d.Role)
                      .WithMany(p => p.Users)
                      .HasForeignKey(d => d.RoleId);
            });

            // ============================
            // 3. RefreshToken Config
            // ============================
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id); // PK is Guid
                
                entity.HasOne(d => d.User)
                      .WithMany(p => p.RefreshTokens)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // 4. Role & Module Config
            // ============================
            modelBuilder.Entity<Role>(e => {
                e.HasKey(x => x.Id);
                e.Property(x => x.RoleName).IsRequired().HasMaxLength(50);
                e.HasIndex(x => x.RoleName).IsUnique();
            });

            modelBuilder.Entity<Module>(e => {
                e.HasKey(x => x.Id);
                e.Property(x => x.ModuleName).IsRequired().HasMaxLength(50);
                e.HasIndex(x => x.ModuleCode).IsUnique();
            });

            // ============================
            // 5. SEED DATA (Fixed GUIDs)
            // ============================
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Static GUIDs taaki har baar same rahein
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var salesRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            
            var modSalesId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var modProdId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

            // 1. Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, RoleName = "Admin", Description = "God Mode", IsActive = true },
                new Role { Id = salesRoleId, RoleName = "SalesUser", Description = "Sales Team", IsActive = true }
            );

            // 2. Modules
            modelBuilder.Entity<Module>().HasData(
                new Module { Id = modSalesId, ModuleName = "Sales", ModuleCode = "SALES", DisplayOrder = 1, IsActive = true },
                new Module { Id = modProdId, ModuleName = "Production", ModuleCode = "PROD", DisplayOrder = 2, IsActive = true }
            );

            // 3. Admin User (Password: Admin@123)
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Username = "admin",
                Email = "admin@myerp.com",
                PasswordHash = "$2a$11$pGW6fhGlCDRZ/BxSmsQagew/WRoFmX5eHPSVXml7dYtux6o.5LluC", // Admin@123 
                RoleId = adminRoleId,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}