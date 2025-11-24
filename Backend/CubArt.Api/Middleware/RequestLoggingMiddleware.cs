using CubArt.Domain.Entities;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Interfaces;
using System.Diagnostics;
using System.Security.Claims;

namespace CubArt.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Получаем scoped сервисы через контекст
            var logRepository = context.RequestServices.GetRequiredService<ISystemLogRepository>();

            try
            {
                await _next(context);
                sw.Stop();

                await LogRequest(context, sw.ElapsedMilliseconds, null, logRepository);
            }
            catch (Exception ex)
            {
                sw.Stop();
                await LogRequest(context, sw.ElapsedMilliseconds, ex, logRepository);
                throw;
            }
            finally
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task LogRequest(HttpContext context, long durationMs, Exception? ex, ISystemLogRepository logRepository)
        {
            try
            {
                string fullExceptionDetails = null;
                if (ex != null)
                {
                    fullExceptionDetails = ex.GetFullExceptionDetails();
                }

                var log = new SystemLog(
                    level: ex != null ? "Error" : "Information",
                    message: $"{context.Request.Method} {context.Request.Path} - {context.Response.StatusCode}",
                    userId: context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: context.Connection.RemoteIpAddress?.ToString(),
                    userAgent: context.Request.Headers["User-Agent"].ToString(),
                    source: "API",
                    action: context.Request.Method,
                    additionalData: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        DurationMs = durationMs,
                        StatusCode = context.Response.StatusCode,
                        QueryString = context.Request.QueryString.Value,
                        Path = context.Request.Path,
                        Method = context.Request.Method,
                        Exception = fullExceptionDetails // Добавляем полные детали исключения
                    }),
                    exceptionType: ex?.GetType().Name
                );

                await logRepository.AddAsync(log);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Ошибка при логировании запроса");
            }
        }
    }
}
