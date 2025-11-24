namespace CubArt.Application.Products.DTOs
{
    public class ProductSpecificationDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Version { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public List<ProductSpecificationItemDto> Items { get; set; } = new();
    }

}
