using CubArt.Domain.Enums;

namespace CubArt.Application.Purchases.DTOs
{
    public class PurchaseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public PurchaseStatusEnum PurchaseStatus { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
