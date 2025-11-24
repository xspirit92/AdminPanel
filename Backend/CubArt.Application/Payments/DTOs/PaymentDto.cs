using CubArt.Domain.Enums;

namespace CubArt.Application.Payments.DTOs
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseId { get; set; }
        public string PurchaseName { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public PaymentStatusEnum PaymentStatus { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
