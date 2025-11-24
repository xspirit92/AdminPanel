using CubArt.Application.Common.Models;
using CubArt.Domain.Entities;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CubArt.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly ISystemLogRepository _logRepository;

        public LoggingBehavior(
            ILogger<LoggingBehavior<TRequest, TResponse>> logger,
            ISystemLogRepository logRepository)
        {
            _logger = logger;
            _logRepository = logRepository;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            string? userId = null;

            _logger.LogInformation("Обработка команды {CommandName} пользователем {UserId}", requestName, userId);

            try
            {
                var result = await next();

                if (result.IsSuccess)
                {
                    await LogSuccessAsync(requestName, request, userId, cancellationToken);
                }
                else
                {
                    await LogFailureAsync(requestName, request, result.ErrorMessage, result.ExceptionMessage, userId, cancellationToken);
                }

                return result;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(requestName, request, ex, userId, cancellationToken);
                throw;
            }
        }

        private async Task LogSuccessAsync(string requestName, TRequest request, string? userId, CancellationToken cancellationToken)
        {
            try
            {
                var log = new SystemLog(
                    "Information", 
                    $"Успешно выполнено: {requestName}", 
                    userId,
                    source: "Application",
                    action: "Execute",
                    entityType: request.GetType().Name,
                    additionalData: GetRequestData(request)
                );

                await _logRepository.AddAsync(log);
            }
            catch (Exception logEx)
            {
                _logger.LogWarning(logEx, "Не удалось записать лог успешного выполнения для {RequestName}", requestName);
            }
        }

        private async Task LogFailureAsync(string requestName, TRequest request, string error, string? exceptionMessage, string? userId, CancellationToken cancellationToken)
        {
            try
            {
                var log = new SystemLog(
                    "Warning", 
                    $"Ошибка выполнения: {requestName} - {error}", 
                    userId,
                    source: "Application",
                    action: "Execute",
                    entityType: request.GetType().Name,
                    additionalData: JsonSerializer.Serialize(new
                    {
                        RequestData = GetRequestData(request),
                        ExceptionMessage = exceptionMessage
                    })
                );

                await _logRepository.AddAsync(log);
            }
            catch (Exception logEx)
            {
                _logger.LogWarning(logEx, "Не удалось записать лог ошибки выполнения для {RequestName}", requestName);
            }
        }

        private async Task LogErrorAsync(string requestName, TRequest request, Exception exception, string? userId, CancellationToken cancellationToken)
        {
            try
            {
                var log = new SystemLog(
                    "Error", 
                    $"Исключение при выполнении: {requestName} - {exception.Message}", 
                    userId,
                    source: "Application",
                    action: "Execute",
                    entityType: request.GetType().Name,
                    exceptionType: exception.GetType().Name,
                    additionalData: JsonSerializer.Serialize(new
                    {
                        RequestData = GetRequestData(request),
                        Exception = exception.GetFullExceptionDetails()
                    })
                );

                await _logRepository.AddAsync(log);
            }
            catch (Exception logEx)
            {
                _logger.LogWarning(logEx, "Не удалось записать лог исключения для {RequestName}", requestName);
            }
        }

        private string GetRequestData(TRequest request)
        {
            try
            {
                // Исключаем чувствительные данные (пароли и т.д.)
                var properties = request.GetType().GetProperties()
                    .Where(p => !p.Name.ToLower().Contains("password"))
                    .ToDictionary(p => p.Name, p => p.GetValue(request)?.ToString() ?? "null");

                return JsonSerializer.Serialize(properties);
            }
            catch
            {
                return "{}";
            }
        }
    }
}
