using CubArt.Application.Common.Models;
using CubArt.Application.Payments.DTOs;
using CubArt.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Payments.Commands
{
    public class CreateOrUpdatePaymentCommand : IRequest<Result<PaymentDto>>
    {
        public Guid? Id { get; init; }
        public Guid PurchaseId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
    }

    public class CreateOrUpdatePaymentCommandValidator : AbstractValidator<CreateOrUpdatePaymentCommand>
    {
        public CreateOrUpdatePaymentCommandValidator()
        {
            RuleFor(x => x.PurchaseId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.PaymentMethod).NotEmpty().IsInEnum();
        }
    }
}
