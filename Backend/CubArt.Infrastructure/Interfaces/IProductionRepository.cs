using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;

namespace CubArt.Infrastructure.Interfaces
{
    public interface IProductionRepository : IRepository<Production, Guid>
    {
    }

}
