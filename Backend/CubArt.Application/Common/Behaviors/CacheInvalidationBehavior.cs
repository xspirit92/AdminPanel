using CubArt.Application.Common.Models;
using CubArt.Application.Products.Commands;
using CubArt.Infrastructure.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CubArt.Application.Common.Behaviors
{
    public class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
    {
        private readonly IRedisCacheService _cache;
        private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;

        public CacheInvalidationBehavior(
            IRedisCacheService cache,
            ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var result = await next();

            // Инвалидируем кэш только после успешных Command
            if (result.IsSuccess && IsCommandRequest(request))
            {
                await InvalidateCache(request);
            }

            return result;
        }

        private static bool IsCommandRequest(TRequest request)
        {
            return request.GetType().Name.EndsWith("Command");
        }

        private async Task InvalidateCache(TRequest request)
        {
            try
            {
                switch (request)
                {
                    case CreateOrUpdateProductCommand command:
                        if (command.Id.HasValue)
                        {
                            await _cache.RemoveAsync(CacheKeys.Product(command.Id.Value));
                        }
                        await _cache.RemoveByPatternAsync("products:*");
                        break;
                }

                _logger.LogDebug("Кэш инвалидирован для {RequestType}", request.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка инвалидации кэша для {RequestType}", request.GetType().Name);
            }
        }
    }

}
