using CubArt.Application.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace CubArt.Application.Suppliers.DTOs
{
    public class SupplierDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Address { get; set; }
    }
}
