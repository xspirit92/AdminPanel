using CubArt.Domain.Common;

namespace CubArt.Domain.Entities
{
    public class Supply : Entity<Guid>, IHasCreatedDate
    {
        public Supply(Guid purchaseId, decimal quantity)
        {
            Id = Guid.NewGuid();
            PurchaseId = purchaseId;
            Quantity = quantity;
        }

        public void UpdateEntity(Guid purchaseId, decimal quantity)
        {
            PurchaseId = purchaseId;
            Quantity = quantity;
        }

        public Guid PurchaseId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime DateCreated { get; set; }

        // Навигационные свойства
        public virtual Purchase Purchase { get; set; }
    }
}
