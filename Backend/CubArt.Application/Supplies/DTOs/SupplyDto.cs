using CubArt.Application.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace CubArt.Application.Supplies.DTOs
{
    public class SupplyDto : BaseActionDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public Guid PurchaseId { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
    }
}
