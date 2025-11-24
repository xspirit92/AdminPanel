using CubArt.Application.Common.Models;
using CubArt.Application.Supplies.DTOs;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Supplies.Commands
{
    public class CreateOrUpdateSupplyCommand : IRequest<Result<SupplyDto>>
    {
        public Guid? Id { get; init; }
        public Guid PurchaseId { get; set; }
        public decimal Quantity { get; set; }
    }

    // Validator
    public class CreateOrUpdateSupplyCommandValidator : AbstractValidator<CreateOrUpdateSupplyCommand>
    {
        public CreateOrUpdateSupplyCommandValidator()
        {
            RuleFor(x => x.PurchaseId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}
