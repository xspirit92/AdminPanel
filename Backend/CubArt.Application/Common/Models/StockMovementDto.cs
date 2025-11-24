namespace CubArt.Application.Common.Models
{
    public class StockMovementDto
    {
        public string FacilityName { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
