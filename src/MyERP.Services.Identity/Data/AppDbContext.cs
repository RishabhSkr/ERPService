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
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue(SystemConstants.StatusPending);
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
            // ═══════════════════════════════════════════════════════════
            // SEED DATA — Absolute minimum to bootstrap the system
            // Everything else (Roles, Modules, Permissions) is created
            // by SuperAdmin through the Settings UI at runtime
            // ═══════════════════════════════════════════════════════════

            // 1. System Roles ONLY (cannot be deleted via API)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = SystemConstants.SuperAdminRoleId, RoleName = SystemConstants.RoleSuperAdmin, Description = "God Mode — undeletable system role", IsActive = true, IsSystemRole = true },
                new Role { Id = SystemConstants.PendingRoleId, RoleName = SystemConstants.RolePending, Description = "Awaiting admin approval — no access", IsActive = true, IsSystemRole = true }
            );

            // 2. SuperAdmin User (Password: Admin@123)
            // This is the ONLY user that can bootstrap the system
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = SystemConstants.SuperAdminUserId,
                Username = "admin",
                Email = "admin@myerp.com",
                PasswordHash = "$2a$11$pGW6fhGlCDRZ/BxSmsQagew/WRoFmX5eHPSVXml7dYtux6o.5LluC", // Admin@123 
                RoleId = SystemConstants.SuperAdminRoleId,
                Status = SystemConstants.StatusActive,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}