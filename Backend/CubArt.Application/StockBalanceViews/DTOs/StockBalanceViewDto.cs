using CubArt.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CubArt.Application.StockBalances.DTOs
{
    public class StockBalanceViewDto
    {
        [Required]
        public string FacilityName { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Required]
        public ProductTypeEnum ProductType { get; set; }
        [Required]
        public UnitOfMeasureEnum UnitOfMeasure { get; set; }
        public decimal StartBalance { get; set; }
        public decimal IncomeBalance { get; set; }
        public decimal OutcomeBalance { get; set; }
        public decimal FinishBalance { get; set; }
        [Required]
        public DateTime BalanceDate { get; set; }
    }

}
