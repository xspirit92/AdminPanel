using CubArt.Domain.Entities;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Infrastructure.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly AppDbContext _context;

        public PurchaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Purchase> GetQueryable()
        {
            return _context.Purchases
                .AsNoTracking()
                .AsQueryable();
        }

        public async Task<Purchase?> GetByIdAsync(Guid id, bool includeRelated = false)
        {
            if (includeRelated)
            {
                return await _context.Purchases
                    .Include(x => x.Facility)
                    .Include(x => x.Supplier)
                    .Include(x => x.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            else
            {
                return await _context.Purchases
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
        }

        public async Task AddAsync(Purchase entity) =>
            await _context.Purchases.AddAsync(entity);

        public void Update(Purchase entity) =>
            _context.Purchases.Update(entity);

        public void Delete(Purchase entity) =>
            _context.Purchases.Remove(entity);
    }
}
