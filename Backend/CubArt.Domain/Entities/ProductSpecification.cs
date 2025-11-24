using CubArt.Domain.Common;

namespace CubArt.Domain.Entities
{
    public class ProductSpecification : Entity<int>, IHasCreatedDate
    {
        public ProductSpecification(int productId, string? version, bool isActive) 
        {
            ProductId = productId;
            Version = version;
            IsActive = isActive;
        }

        public int ProductId { get; set; }
        public string? Version { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }

        // Навигационные свойства
        public virtual Product Product { get; set; }
        public virtual ICollection<ProductSpecificationItem> Items { get; set; } = new List<ProductSpecificationItem>();
    }
}
