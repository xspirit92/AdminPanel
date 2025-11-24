using CubArt.Domain.Enums;

namespace CubArt.Infrastructure.Models
{
    public record UpdateStockMovementsAndRecalculateBalancesModel(
        Guid ReferenceId,
        OperationTypeEnum OperationType,
        StockMovemetReferenceTypeEnum ReferenceType, 
        DateTime Date, 
        int FacilityId, 
        int ProductId, 
        decimal Quantity
    );
}
