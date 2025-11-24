using CubArt.Domain.Entities;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Infrastructure.Repositories
{
    public class ProductionRepository : IProductionRepository
    {
        private readonly AppDbContext _context;

        public ProductionRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Production> GetQueryable()
        {
            return _context.Productions
                .AsNoTracking()
                .AsQueryable();
        }

        public async Task<Production?> GetByIdAsync(Guid id, bool includeRelated = false)
        {
            if (includeRelated)
            {
                return await _context.Productions
                    .Include(x => x.Facility)
                    .Include(x => x.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            else
            {
                return await _context.Productions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
        }

        public async Task AddAsync(Production entity) =>
            await _context.Productions.AddAsync(entity);

        public void Update(Production entity) =>
            _context.Productions.Update(entity);

        public void Delete(Production entity) =>
            _context.Productions.Remove(entity);
    }
}
