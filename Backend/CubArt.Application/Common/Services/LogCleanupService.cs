using CubArt.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CubArt.Application.Common.Services
{
    public class LogCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LogCleanupService> _logger;

        public LogCleanupService(IServiceProvider serviceProvider, ILogger<LogCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var logRepository = scope.ServiceProvider.GetRequiredService<ISystemLogRepository>();

                    var deleteBefore = DateTime.UtcNow.AddMonths(-6); // Храним 6 месяцев
                    await logRepository.CleanOldLogsAsync(deleteBefore, stoppingToken);

                    _logger.LogInformation("Очистка логов выполнена. Удалены логи старше {Date}", deleteBefore);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при очистке логов");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken); // Запускаем раз в день
            }
        }
    }

}
