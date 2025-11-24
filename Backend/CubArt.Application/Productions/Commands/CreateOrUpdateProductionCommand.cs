using CubArt.Application.Common.Models;
using CubArt.Application.Productions.DTOs;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Productions.Commands
{
    public class CreateOrUpdateProductionCommand : IRequest<Result<ProductionDto>>
    {
        public Guid? Id { get; init; }
        public int ProductId { get; set; }
        public int FacilityId { get; set; }
        public decimal Quantity { get; set; }
    }

    // Validator
    public class CreateOrUpdateProductionCommandValidator : AbstractValidator<CreateOrUpdateProductionCommand>
    {
        public CreateOrUpdateProductionCommandValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.FacilityId).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}
