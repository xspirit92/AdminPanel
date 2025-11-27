using CubArt.Application.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace CubArt.Application.Productions.DTOs
{
    public class ProductionDto : BaseActionDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Required]
        public string FacilityName { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
    }
}
