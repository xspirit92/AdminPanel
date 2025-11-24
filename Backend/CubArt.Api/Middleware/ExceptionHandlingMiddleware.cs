using CubArt.Domain.Exceptions;
using System.Text.Json;

namespace CubArt.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Объект не найден");
                await HandleExceptionAsync(context, ex, StatusCodes.Status404NotFound);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain validation error");
                await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка");
                await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // Собираем полную информацию об исключении
            var exceptionDetails = GetExceptionDetails(exception);

            var response = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = GetTitleForStatusCode(statusCode),
                status = statusCode,
                detail = exception.Message,
                instance = context.Request.Path.ToString(),
                errors = exceptionDetails.Errors,
                stackTrace = exceptionDetails.StackTrace,
                innerException = exceptionDetails.InnerException
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }

        private static (string[] Errors, string StackTrace, object InnerException) GetExceptionDetails(Exception exception)
        {
            var errors = new List<string> { exception.Message };
            var currentException = exception;

            // Собираем все внутренние исключения
            while (currentException.InnerException != null)
            {
                currentException = currentException.InnerException;
                errors.Add($"Inner: {currentException.Message}");
            }

            string stackTrace = null;
            object innerException = null;

            stackTrace = exception.StackTrace;

            if (exception.InnerException != null)
            {
                innerException = new
                {
                    message = exception.InnerException.Message,
                    type = exception.InnerException.GetType().Name,
                    stackTrace = exception.InnerException.StackTrace
                };
            }

            return (errors.ToArray(), stackTrace, innerException);
        }


        private static string GetTitleForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                404 => "Not Found",
                500 => "Internal Server Error",
                _ => "An error occurred"
            };
        }
    }

}
