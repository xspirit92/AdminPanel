using CubArt.Application.Common.Models;
using CubArt.Application.Common.Services;
using CubArt.Application.Users.Commands;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Users.Handlers
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IAuthService authService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _authService = authService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _authService.GetCurrentUserIdAsync();

                // Находим пользователя
                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null)
                {
                    return Result.Failure("Пользователь не найден");
                }

                if (!user.IsActive)
                {
                    return Result.Failure("Учетная запись пользователя деактивирована");
                }

                // Проверяем текущий пароль
                if (!_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    return Result.Failure("Текущий пароль указан неверно");
                }

                // Хешируем новый пароль
                var newPasswordHash = _authService.HashPassword(request.NewPassword);

                // Обновляем пароль
                user.ChangePassword(newPasswordHash);
                _userRepository.Update(user);

                await _unitOfWork.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Ошибка при смене пароля: {ex.Message}", ex);
            }
        }
    }

}
