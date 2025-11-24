using CubArt.Domain.Enums;

namespace CubArt.Application.StockBalances.DTOs
{
    public class StockBalanceViewDto
    {
        public string FacilityName { get; set; }
        public string ProductName { get; set; }
        public ProductTypeEnum ProductType { get; set; }
        public UnitOfMeasureEnum UnitOfMeasure { get; set; }
        public decimal StartBalance { get; set; }
        public decimal IncomeBalance { get; set; }
        public decimal OutcomeBalance { get; set; }
        public decimal FinishBalance { get; set; }
        public DateTime BalanceDate { get; set; }
    }

}
