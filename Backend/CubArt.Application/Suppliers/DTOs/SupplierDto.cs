using CubArt.Application.Common.Models;

namespace CubArt.Application.Suppliers.DTOs
{
    public class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
    }
}
