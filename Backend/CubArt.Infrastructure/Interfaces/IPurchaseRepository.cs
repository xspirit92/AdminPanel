using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;

namespace CubArt.Infrastructure.Interfaces
{
    public interface IPurchaseRepository : IRepository<Purchase, Guid>
    {
    }

}
