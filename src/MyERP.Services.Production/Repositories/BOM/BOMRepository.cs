/*
 * BOM Repository Implementation
 */

using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.BOM
{
    public class BOMRepository : IBOMRepository
    {
        private readonly ProductionDbContext _context;

        public BOMRepository(ProductionDbContext context)
        {
            _context = context;
        }

        public async Task<Models.BOM?> GetByIdAsync(Guid bomId)
        {
            return await _context.BOMs.FindAsync(bomId);
        }

        public async Task<Models.BOM?> GetByIdWithLinesAsync(Guid bomId)
        {
            return await _context.BOMs
                .Include(b => b.Lines.OrderBy(l => l.LineNumber))
                .FirstOrDefaultAsync(b => b.BOMId == bomId);
        }

        public async Task<Models.BOM?> GetActiveByProductIdAsync(Guid productId)
        {
            return await _context.BOMs
                .Include(b => b.Lines.OrderBy(l => l.LineNumber))
                .FirstOrDefaultAsync(b => b.ProductId == productId && b.IsActive);
        }

        public async Task<IEnumerable<Models.BOM>> GetAllAsync()
        {
            return await _context.BOMs
                .Include(b => b.Lines)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string bomCode)
        {
            return await _context.BOMs.AnyAsync(b => b.BomCode == bomCode);
        }

        public async Task<Models.BOM> CreateAsync(Models.BOM bom)
        {
            _context.BOMs.Add(bom);
            await _context.SaveChangesAsync();
            return bom;
        }

        public async Task<Models.BOM> UpdateAsync(Models.BOM bom)
        {
            bom.UpdatedAt = DateTime.UtcNow;
            _context.BOMs.Update(bom);
            await _context.SaveChangesAsync();
            return bom;
        }

        public async Task DeleteAsync(Guid bomId)
        {
            var bom = await _context.BOMs.FindAsync(bomId);
            if (bom != null)
            {
                _context.BOMs.Remove(bom);
                await _context.SaveChangesAsync();
            }
        }
    }
}
