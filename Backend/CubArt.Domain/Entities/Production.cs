using CubArt.Domain.Common;

namespace CubArt.Domain.Entities
{
    public class Production : Entity<Guid>, IHasCreatedDate
    {
        public Production(int productId, int facilityId, decimal quantity)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            FacilityId = facilityId;
            Quantity = quantity;
        }

        public void UpdateEntity(int productId, int facilityId, decimal quantity)
        {
            ProductId = productId;
            FacilityId = facilityId;
            Quantity = quantity;
        }

        public int ProductId { get; set; }
        public int FacilityId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime DateCreated { get; set; }

        // Навигационные свойства
        public virtual Product Product { get; set; }
        public virtual Facility Facility { get; set; }
    }
}
