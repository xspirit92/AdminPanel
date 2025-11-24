using CubArt.Application.Common.Models;
using CubArt.Application.Common.Services;
using CubArt.Application.Users.Commands;
using CubArt.Application.Users.DTOs;
using CubArt.Domain.Entities;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Users.Handlers
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IAuthService authService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _authService = authService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Проверяем уникальность username
                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    return Result.Failure<UserDto>($"Пользователь с именем '{request.Username}' уже существует");
                }

                // Проверяем уникальность email
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    return Result.Failure<UserDto>($"Пользователь с email '{request.Email}' уже существует");
                }

                // Хешируем пароль
                var passwordHash = _authService.HashPassword(request.Password);

                // Создаем пользователя
                var user = new User(
                    request.Username,
                    request.Email,
                    passwordHash,
                    request.FirstName,
                    request.LastName
                );

                await _userRepository.AddAsync(user);
                await _unitOfWork.CommitAsync(cancellationToken);

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

                return Result.Success(userDto);
            }
            catch (Exception ex)
            {
                return Result.Failure<UserDto>($"Ошибка при создании пользователя: {ex.Message}", ex);
            }
        }
    }

}
