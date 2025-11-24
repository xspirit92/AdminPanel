using CubArt.Application.Common.Models;
using CubArt.Application.Purchases.DTOs;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Purchases.Commands
{
    public class CreateOrUpdatePurchaseCommand : IRequest<Result<PurchaseDto>>
    {
        public Guid? Id { get; init; }
        public int SupplierId { get; set; }
        public int FacilityId { get; set; }
        public int ProductId { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
    }

    // Validator
    public class CreateOrUpdatePurchaseCommandValidator : AbstractValidator<CreateOrUpdatePurchaseCommand>
    {
        public CreateOrUpdatePurchaseCommandValidator()
        {
            RuleFor(x => x.SupplierId).GreaterThan(0);
            RuleFor(x => x.FacilityId).GreaterThan(0);
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}
