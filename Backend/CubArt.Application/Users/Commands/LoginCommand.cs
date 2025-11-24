using CubArt.Application.Common.Models;
using CubArt.Application.Users.DTOs;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Users.Commands
{
    public class LoginCommand : IRequest<Result<LoginDto>>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // Validator
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Username).NotNull();
            RuleFor(x => x.Password).NotNull();
        }
    }
}
