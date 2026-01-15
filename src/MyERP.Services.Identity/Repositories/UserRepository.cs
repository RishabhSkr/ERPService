using Microsoft.EntityFrameworkCore;
using MyERP.Services.Identity.Data;
using MyERP.Services.Identity.Models;
using MyERP.Services.Identity.DTOs.Auth;

namespace MyERP.Services.Identity.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role) // Role include karna zaroori hai
                .FirstOrDefaultAsync(u => u.Username == username);
        }
        
        public async Task<List<ModuleDto>> GetAccessibleModulesAsync(Guid roleId)
        {
            // Ye complex query hai jo RolePermissions se Modules nikalegi
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.IsGranted)
                .Include(rp => rp.Module)
                .Select(rp => rp.Module)
                // .Where(m => m != null) // Fix: Filter out null modules
                .Distinct()
                .OrderBy(m => m!.DisplayOrder) // Fix: m is not null here
                .Select(m => new ModuleDto
                {
                    ModuleId = m!.Id, 
                    ModuleName = m.ModuleName,
                    ModuleCode = m.ModuleCode,
                    DisplayOrder = m.DisplayOrder,
                    HasAccess = true
                })
                .ToListAsync();
        }
        // --- Refresh Token Implementation ---
        public async Task SaveRefreshTokenAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u!.Role) // Fix: Compiler warning for nullable User
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken token)
        {
            token.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            // User ke saath Role bhi load kar rahe hain
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive) // Filter Active
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive) // Filter Active
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                // Soft Delete
                user.IsActive = false;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string?> GetRoleNameAsync(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            return role?.RoleName;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Include(r => r.Users)
                .Include(r => r.RolePermissions)
                .Where(r => r.IsActive) // Filter Active
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(Guid roleId)
        {
            return await _context.Roles
                .Include(r => r.Users)
                .Include(r => r.RolePermissions)
                .Where(r => r.IsActive) // Filter Active
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task CreateRoleAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(Role role)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRoleAsync(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role != null)
            {
                // Soft Delete
                role.IsActive = false;
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<RolePermission>> GetPermissionsByRoleIdAsync(Guid roleId)
        {
            return await _context.RolePermissions
                .Include(rp => rp.Module)
                .Where(rp => rp.RoleId == roleId && rp.IsGranted) // Only Granted
                .ToListAsync();
        }

        public async Task<RolePermission?> GetPermissionByIdAsync(Guid rolePermissionId)
        {
             return await _context.RolePermissions.FindAsync(rolePermissionId);
        }

        public async Task<RolePermission?> GetPermissionAsync(Guid roleId, Guid moduleId, string permissionName)
        {
            return await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.ModuleId == moduleId && rp.PermissionName == permissionName);
        }

        public async Task GrantPermissionAsync(RolePermission permission)
        {
            var existing = await GetPermissionAsync(permission.RoleId, permission.ModuleId, permission.PermissionName);
            if (existing != null)
            {
                existing.IsGranted = true;
                _context.RolePermissions.Update(existing);
            }
            else
            {
                permission.IsGranted = true; // Ensure true
                await _context.RolePermissions.AddAsync(permission);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemovePermissionAsync(RolePermission permission)
        {
            // Soft Delete (Revoke)
            permission.IsGranted = false;
            _context.RolePermissions.Update(permission);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Module>> GetAllModulesAsync()
        {
            return await _context.Modules.ToListAsync();
        }

        public async Task<bool> HasPermissionAsync(Guid roleId, string path, string method)
        {
            // Normalize
            path = path.ToLower();
            method = method.ToUpper();

            // Simple check: Exact match or Prefix Match (logic depends on how strictly you define endpoints)
            // Ideally: endpoint: "/api/users" should match currentPath "/api/users" and "/api/users/guid"
            
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.IsGranted && rp.HttpMethod == method)
                .Select(rp => rp.ApiEndpoint.ToLower())
                .ToListAsync();

            foreach (var endpoint in permissions)
            {
                if (path == endpoint || path.StartsWith(endpoint + "/"))
                {
                    return true;
                }
            }
            return false;
        }

    }
}