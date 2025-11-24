using CubArt.Domain.Entities;

namespace CubArt.Domain.Events
{
    public class PurchaseCreatedEvent : DomainEvent
    {
        public PurchaseCreatedEvent(Purchase order)
        {
            Purchase = order;
        }

        public Purchase Purchase { get; }
    }

    public class RawMaterialStockIncreasedEvent : DomainEvent
    {
        public RawMaterialStockIncreasedEvent(int productId, decimal quantity, int facilityId)
        {
            ProductId = productId;
            Quantity = quantity;
            FacilityId = facilityId;
        }

        public int ProductId { get; }
        public decimal Quantity { get; }
        public int FacilityId { get; }
    }
}
