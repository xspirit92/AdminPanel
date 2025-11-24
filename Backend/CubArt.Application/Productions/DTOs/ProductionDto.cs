using CubArt.Application.Common.Models;

namespace CubArt.Application.Productions.DTOs
{
    public class ProductionDto : BaseActionDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string FacilityName { get; set; }
        public decimal Quantity { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
