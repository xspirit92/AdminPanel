using CubArt.Domain.Common;
using CubArt.Domain.Enums;

namespace CubArt.Domain.Entities
{
    public class Product : Entity<int>
    {
        public Product(string name, ProductTypeEnum productType, UnitOfMeasureEnum unitOfMeasure)
        {
            Name = name;
            ProductType = productType;
            UnitOfMeasure = unitOfMeasure;
        }

        public string Name { get; set; }
        public ProductTypeEnum ProductType { get; set; }
        public UnitOfMeasureEnum UnitOfMeasure { get; set; }

        // Навигационные свойства
        public virtual ICollection<ProductSpecification> ProductSpecifications { get; set; } = new List<ProductSpecification>();
    }
}
