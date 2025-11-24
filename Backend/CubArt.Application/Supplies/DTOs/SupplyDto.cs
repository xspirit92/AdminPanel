using CubArt.Application.Common.Models;

namespace CubArt.Application.Supplies.DTOs
{
    public class SupplyDto : BaseActionDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
