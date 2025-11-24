using CubArt.Domain.Enums;

namespace CubArt.Application.Products.DTOs
{
    public class ProductSpecificationItemDto
    {
        public int Id { get; set; }
        public int ProductSpecificationId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public ProductTypeEnum ProductType { get; set; }
        public decimal Quantity { get; set; }
        public UnitOfMeasureEnum UnitOfMeasure { get; set; }
    }

}
