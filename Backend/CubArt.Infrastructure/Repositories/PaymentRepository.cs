using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Infrastructure.Common;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Infrastructure.Repositories
{
    public class PaymentRepository : Repository<Payment, Guid>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<decimal> GetTotalPaidAmountAsync(Guid purchaseId, Guid? paymentId = null)
        {
            return await _dbSet
                .Where(p => p.PurchaseId == purchaseId && p.PaymentStatus == PaymentStatusEnum.Completed &&
                    (paymentId == null || p.Id != paymentId))
                .SumAsync(p => p.Amount);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByPurchaseIdAsync(Guid purchaseId)
        {
            return await _dbSet
                .Where(p => p.PurchaseId == purchaseId)
                .OrderByDescending(p => p.DateCreated)
                .AsNoTracking()
                .ToListAsync();
        }
    }

}
