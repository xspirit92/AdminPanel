using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;

namespace CubArt.Infrastructure.Interfaces
{
    public interface IProductRepository : IRepository<Product, int>
    {
    }

}
