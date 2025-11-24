using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;

namespace CubArt.Infrastructure.Interfaces
{
    public interface ISupplyRepository : IRepository<Supply, Guid>
    {
        Task<decimal> GetTotalIncomeQuantityAsync(Guid purchaseId, Guid? sypplyId = null);
        Task<IEnumerable<Supply>> GetSuppliesByPurchaseIdAsync(Guid purchaseId);
    }

}
