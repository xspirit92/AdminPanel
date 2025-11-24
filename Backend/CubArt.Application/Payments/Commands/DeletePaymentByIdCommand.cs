using CubArt.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Payments.Commands
{
    public class DeletePaymentByIdCommand : IRequest<Result>
    {
        public Guid Id { get; init; }
    }

    public class DeletePaymentByIdCommandValidator : AbstractValidator<DeletePaymentByIdCommand>
    {
        public DeletePaymentByIdCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
