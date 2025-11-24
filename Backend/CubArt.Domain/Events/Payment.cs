using CubArt.Domain.Entities;

namespace CubArt.Domain.Events
{
    public class PaymentCreatedEvent : DomainEvent
    {
        public PaymentCreatedEvent(Payment payment)
        {
            Payment = payment;
        }

        public Payment Payment { get; }
    }
}
