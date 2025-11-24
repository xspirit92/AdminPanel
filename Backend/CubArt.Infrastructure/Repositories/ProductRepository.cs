using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product, int>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<Product?> GetByIdAsync(int id, bool includeRelated = false)
        {
            if (includeRelated)
            {
                return await _dbSet
                    .Include(p => p.ProductSpecifications)
                    .ThenInclude(ps => ps.Items)
                    .ThenInclude(psi => psi.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            return await base.GetByIdAsync(id, includeRelated);
        }
    }

}
