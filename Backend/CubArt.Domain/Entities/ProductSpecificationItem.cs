using CubArt.Domain.Common;

namespace CubArt.Domain.Entities
{
    public class ProductSpecificationItem : Entity<int>
    {
        public ProductSpecificationItem(int productSpecificationId, int productId, decimal quantity) 
        {
            ProductSpecificationId = productSpecificationId;
            ProductId = productId;
            Quantity = quantity;
        }

        public int ProductSpecificationId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }

        // Навигационные свойства
        public virtual ProductSpecification ProductSpecification { get; set; }
        public virtual Product Product { get; set; }
    }
}
