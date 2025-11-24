using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Infrastructure.Models;

namespace CubArt.Infrastructure.Interfaces
{
    public interface IStockMovementService
    {
        Task RecalculateAllBalancesFromDate(DateTime date, int? facilityId = null, int? productId = null, CancellationToken cancellationToken = default);

        Task<DateTime> GetLastBalanceDate();

        Task<List<StockMovement>> GetStockMovementsByReference(string referenceId, StockMovemetReferenceTypeEnum referenceType);

        Task<StockBalanceView> GetStockBalanceByDate(int? facilityId, int? productId, DateTime BalanceDate);

        Task UpdateStockMovementsAndRecalculateBalances(UpdateStockMovementsAndRecalculateBalancesModel model);

        Task DeleteStockMovements(Guid referenceId, StockMovemetReferenceTypeEnum referenceType);
    }
}
