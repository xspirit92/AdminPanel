using CubArt.Application.Common.Models;
using CubArt.Application.Common.Services;
using CubArt.Application.Users.Commands;
using CubArt.Application.Users.DTOs;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Users.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public LoginCommandHandler(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<Result<LoginDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(request.Username);
                if (user == null || !user.IsActive)
                {
                    return Result.Failure<LoginDto>("Неверное имя пользователя или пароль");
                }

                if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Result.Failure<LoginDto>("Неверное имя пользователя или пароль");
                }

                // Обновляем время последнего входа
                user.UpdateLastLogin();
                _userRepository.Update(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    DateCreated = user.DateCreated,
                    LastLogin = user.LastLogin
                };

                var token = _authService.GenerateJwtToken(userDto);

                var response = new LoginDto
                {
                    Token = token,
                    User = userDto,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<LoginDto>($"Ошибка при входе: {ex.Message}", ex);
            }
        }
    }

}
