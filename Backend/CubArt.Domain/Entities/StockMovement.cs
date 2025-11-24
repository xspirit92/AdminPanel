using CubArt.Domain.Common;
using CubArt.Domain.Enums;

namespace CubArt.Domain.Entities
{
    public class StockMovement : Entity<Guid>, IHasCreatedDate
    {
        public StockMovement(
            int facilityId,
            int productId,
            OperationTypeEnum operationType,
            StockMovemetReferenceTypeEnum referenceType,
            string referenceId,
            decimal quantity,
            DateTime dateCreated)
        {
            FacilityId = facilityId;
            ProductId = productId;
            OperationType = operationType;
            ReferenceType = referenceType;
            ReferenceId = referenceId;
            Quantity = quantity;
            DateCreated = dateCreated;
        }

        public void UpdateEntity(
            int facilityId,
            int productId,
            OperationTypeEnum operationType,
            StockMovemetReferenceTypeEnum referenceType,
            string referenceId,
            decimal quantity,
            DateTime date)
        {
            FacilityId = facilityId;
            ProductId = productId;
            OperationType = operationType;
            ReferenceType = referenceType;
            ReferenceId = referenceId;
            Quantity = quantity;
            DateCreated = date;
        }

        public int FacilityId { get; set; }
        public int ProductId { get; set; }
        public OperationTypeEnum OperationType { get; set; }
        public StockMovemetReferenceTypeEnum ReferenceType { get; set; }
        public string ReferenceId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime DateCreated { get; set; }

        // Навигационные свойства
        public virtual Facility Facility { get; set; }
        public virtual Product Product { get; set; }
    }
}
