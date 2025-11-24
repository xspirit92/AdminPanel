using CubArt.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Users.Commands
{
    public class ChangePasswordCommand : IRequest<Result>
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    // Validator
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(100);

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(100)
                .Must((command, newPassword) => newPassword != command.CurrentPassword)
                .WithMessage("Новый пароль должен отличаться от текущего");
        }
    }
}
