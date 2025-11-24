using CubArt.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Purchases.Commands
{
    public class DeletePurchaseByIdCommand : IRequest<Result>
    {
        public Guid Id { get; init; }
    }

    // Validator
    public class DeletePurchaseCommandValidator : AbstractValidator<DeletePurchaseByIdCommand>
    {
        public DeletePurchaseCommandValidator()
        {
            RuleFor(x => x.Id).NotNull();
        }
    }
}
