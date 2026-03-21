/*
 * PendingRequest Repository Implementation
 */

using Microsoft.EntityFrameworkCore;
using MyERP.Services.Production.Data;
using MyERP.Services.Production.Models;

namespace MyERP.Services.Production.Repositories.PendingRequests
{
    public class PendingRequestRepository : IPendingRequestRepository
    {
        private readonly ProductionDbContext _context;

        public PendingRequestRepository(ProductionDbContext context)
        {
            _context = context;
        }

        public async Task<PendingRequest?> GetByIdAsync(Guid id)
        {
            return await _context.PendingRequests.FindAsync(id);
        }

        public async Task<PendingRequest?> GetByIdWithItemsAsync(Guid id)
        {
            return await _context.PendingRequests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<PendingRequest>> GetAllAsync()
        {
            return await _context.PendingRequests
                .Include(r => r.Items)
                .OrderByDescending(r => r.ReceivedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PendingRequest>> GetByStatusAsync(string status)
        {
            return await _context.PendingRequests
                .Include(r => r.Items)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.ReceivedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsBySalesOrderIdAsync(Guid salesOrderId)
        {
            return await _context.PendingRequests
                .AnyAsync(r => r.SalesOrderId == salesOrderId);
        }

        public async Task<PendingRequest> CreateAsync(PendingRequest request)
        {
            _context.PendingRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<PendingRequest> UpdateAsync(PendingRequest request)
        {
            _context.PendingRequests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }
    }
}
