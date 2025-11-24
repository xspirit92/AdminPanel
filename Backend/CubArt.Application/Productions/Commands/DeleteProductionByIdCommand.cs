using CubArt.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Productions.Commands
{
    public class DeleteProductionByIdCommand : IRequest<Result>
    {
        public Guid Id { get; init; }
    }

    // Validator
    public class DeleteProductionByIdValidator : AbstractValidator<DeleteProductionByIdCommand>
    {
        public DeleteProductionByIdValidator()
        {
            RuleFor(x => x.Id).NotNull();
        }
    }
}
