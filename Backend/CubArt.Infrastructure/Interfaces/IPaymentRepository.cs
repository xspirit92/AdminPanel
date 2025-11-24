using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;

namespace CubArt.Infrastructure.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment, Guid>
    {
        Task<decimal> GetTotalPaidAmountAsync(Guid purchaseId, Guid? paymentId = null);
        Task<IEnumerable<Payment>> GetPaymentsByPurchaseIdAsync(Guid purchaseId);
    }

}
