using Cronos;
using CubArt.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CubArt.Application.Common.BackgroundServices
{
    public class StockBalanceRecalculationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockBalanceRecalculationService> _logger;

        public StockBalanceRecalculationService(
            IServiceProvider serviceProvider,
            ILogger<StockBalanceRecalculationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stockMovementService = scope.ServiceProvider.GetRequiredService<IStockMovementService>();
                
                var date = await stockMovementService.GetLastBalanceDate();
                await stockMovementService.RecalculateAllBalancesFromDate(date, cancellationToken: stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // Запускаем в 00:00 каждый день для расчета балансов за предыдущий день
                        await WaitForNextSchedule("0 21 * * *"); //utc

                        date = await stockMovementService.GetLastBalanceDate();
                        await stockMovementService.RecalculateAllBalancesFromDate(date, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при пересчете балансов");
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Повтор через час при ошибке
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при пересчете балансов");
            }
        }
        private async Task WaitForNextSchedule(string cronExpression)
        {
            var parsedExp = CronExpression.Parse(cronExpression);
            var currentUtcTime = DateTimeOffset.UtcNow.UtcDateTime;
            var occurenceTime = parsedExp.GetNextOccurrence(currentUtcTime);

            var delay = occurenceTime.GetValueOrDefault() - currentUtcTime;
            var message = string.Format($"{nameof(StockBalanceRecalculationService)} запустится через {0:%d} дней {0:hh\\:mm\\:ss}", delay);
            

            await Task.Delay(delay);
        }
    }
}
