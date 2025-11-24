using CubArt.Domain.Common;

namespace CubArt.Domain.Entities
{
    public class StockBalance : Entity<Guid>, IHasCreatedDate
    {
        public StockBalance(
            int facilityId,
            int productId,
            decimal startBalance,
            decimal incomeBalance,
            decimal outcomeBalance,
            decimal finishBalance,
            DateTime dateCreated)
        {
            FacilityId = facilityId;
            ProductId = productId;
            StartBalance = startBalance;
            IncomeBalance = incomeBalance;
            OutcomeBalance = outcomeBalance;
            FinishBalance = finishBalance;
            DateCreated = dateCreated;
        }

        public void UpdateBalances(decimal startBalance, decimal income, decimal outcome, decimal finishBalance)
        {
            StartBalance = startBalance;
            IncomeBalance = income;
            OutcomeBalance = outcome;
            FinishBalance = finishBalance;
        }

        public int FacilityId { get; set; }
        public int ProductId { get; set; }
        public decimal StartBalance { get; set; }
        public decimal IncomeBalance { get; set; }
        public decimal OutcomeBalance { get; set; }
        public decimal FinishBalance { get; set; }
        public DateTime DateCreated { get; set; }

        // Навигационные свойства
        public virtual Facility Facility { get; set; }
        public virtual Product Product { get; set; }
    }
}
