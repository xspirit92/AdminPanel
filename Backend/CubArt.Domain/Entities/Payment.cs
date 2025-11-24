using CubArt.Domain.Common;
using CubArt.Domain.Enums;
using CubArt.Domain.Events;

namespace CubArt.Domain.Entities
{
    public class Payment : Entity<Guid>, IHasCreatedDate
    {
        public Payment(Guid purchaseId, decimal amount, PaymentMethodEnum paymentMethod)
        {
            PurchaseId = purchaseId;
            Amount = amount;
            PaymentMethod = paymentMethod;
            PaymentStatus = PaymentStatusEnum.Completed;

            AddDomainEvent(new PaymentCreatedEvent(this));
        }

        public void UpdateEntity(Guid purchaseId, decimal amount, PaymentMethodEnum paymentMethod)
        {
            PurchaseId = purchaseId;
            Amount = amount;
            PaymentMethod = paymentMethod;
        }

        public Guid PurchaseId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public PaymentStatusEnum PaymentStatus { get; set; }
        public string? Сomment { get; set; }
        public DateTime DateCreated { get; set; }

        // Навигационные свойства
        public virtual Purchase Purchase { get; set; }
    }
}
