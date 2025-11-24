using CubArt.Domain.Enums;

namespace CubArt.Application.Products.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProductTypeEnum ProductType { get; set; }
        public UnitOfMeasureEnum UnitOfMeasure { get; set; }
        public ProductSpecificationDto? ActiveSpecification { get; set; }

    }

}
