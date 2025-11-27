using CubArt.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CubArt.Application.Payments.DTOs
{
    public class PaymentDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public Guid PurchaseId { get; set; }
        [Required]
        public string PurchaseName { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public PaymentMethodEnum PaymentMethod { get; set; }
        [Required]
        public PaymentStatusEnum PaymentStatus { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
    }
}
