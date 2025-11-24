using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Infrastructure.Repositories
{
    public class SupplyRepository : Repository<Supply, Guid>, ISupplyRepository
    {
        public SupplyRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<decimal> GetTotalIncomeQuantityAsync(Guid purchaseId, Guid? supplyId = null)
        {
            return await _dbSet
                .Where(p => p.PurchaseId == purchaseId &&
                    (supplyId == null || p.Id != supplyId))
                .SumAsync(p => p.Quantity);
        }

        public async Task<IEnumerable<Supply>> GetSuppliesByPurchaseIdAsync(Guid purchaseId)
        {
            return await _dbSet
                .Where(p => p.PurchaseId == purchaseId)
                .OrderByDescending(p => p.DateCreated)
                .AsNoTracking()
                .ToListAsync();
        }
    }

}
