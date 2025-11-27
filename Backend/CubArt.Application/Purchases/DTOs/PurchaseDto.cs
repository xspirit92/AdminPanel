using CubArt.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CubArt.Application.Purchases.DTOs
{
    public class PurchaseDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int SupplierId { get; set; }
        [Required]
        public string SupplierName { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Required]
        public int FacilityId { get; set; }
        [Required]
        public string FacilityName { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public PurchaseStatusEnum PurchaseStatus { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
    }
}
