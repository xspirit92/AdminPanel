using CubArt.Domain.Common;
using CubArt.Domain.Enums;
using CubArt.Domain.Events;

namespace CubArt.Domain.Entities
{
    public class Purchase : Entity<Guid>, IHasCreatedDate
    {
        public Purchase(int productId, int supplierId, int facilityId, decimal amount, decimal quantity)
        {
            ProductId = productId;
            SupplierId = supplierId;
            FacilityId = facilityId;
            Amount = amount;
            Quantity = quantity;
            PurchaseStatus = PurchaseStatusEnum.Pending;

            AddDomainEvent(new PurchaseCreatedEvent(this));
        }

        public void UpdateEntity(int productId, int supplierId, int facilityId, decimal amount, decimal quantity)
        {
            ProductId = productId;
            SupplierId = supplierId;
            FacilityId = facilityId;
            Amount = amount;
            Quantity = quantity;
        }

        public int SupplierId { get; set; }
        public int ProductId { get; set; }
        public int FacilityId { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public PurchaseStatusEnum PurchaseStatus { get; set; }
        public DateTime DateCreated { get; set; }

        // Навигационные свойства
        public virtual Supplier Supplier { get; set; }
        public virtual Product Product { get; set; }
        public virtual Facility Facility { get; set; }
    }
}
