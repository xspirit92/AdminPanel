using CubArt.Application.Common.Models;
using CubArt.Application.Users.DTOs;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Users.Commands
{
    public class CreateUserCommand : IRequest<Result<UserDto>>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    // Validator
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MaximumLength(50)
                .Matches("^[a-zA-Z0-9_]+$")
                .WithMessage("Имя пользователя может содержать только буквы, цифры и подчеркивания");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(100);
        }
    }
}
