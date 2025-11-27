using CubArt.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CubArt.Application.Products.DTOs
{
    public class ProductDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public ProductTypeEnum ProductType { get; set; }
        [Required]
        public UnitOfMeasureEnum UnitOfMeasure { get; set; }
        public ProductSpecificationDto? ActiveSpecification { get; set; }

    }

}
