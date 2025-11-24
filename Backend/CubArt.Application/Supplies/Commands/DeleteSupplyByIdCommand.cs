using CubArt.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Supplies.Commands
{
    public class DeleteSupplyByIdCommand : IRequest<Result>
    {
        public Guid Id { get; init; }
    }

    // Validator
    public class DeleteSupplyByIdCommandValidator : AbstractValidator<DeleteSupplyByIdCommand>
    {
        public DeleteSupplyByIdCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
