using CubArt.Application.Common.Models;
using CubArt.Application.Products.Queries;
using CubArt.Infrastructure.Caching;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CubArt.Application.Common.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
    {
        private readonly IRedisCacheService _cache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(
            IRedisCacheService cache,
            ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Кэшируем только Query
            if (!IsQueryRequest(request))
                return await next();

            var cacheKey = GetCacheKey(request);
            if (string.IsNullOrEmpty(cacheKey))
                return await next();

            try
            {
                // Пробуем получить из кэша
                var cachedResult = await _cache.GetAsync<TResponse>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogDebug("Возвращаем данные из кэша для {CacheKey}", cacheKey);
                    return cachedResult;
                }

                // Если нет в кэше - выполняем запрос
                var result = await next();

                // Сохраняем в кэш если успешно
                if (result.IsSuccess)
                {
                    var expiry = GetCacheExpiry(request);
                    await _cache.SetAsync(cacheKey, result, expiry);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка кэширования для {CacheKey}", cacheKey);
                return await next(); // При ошибках кэша просто выполняем запрос
            }
        }

        private static bool IsQueryRequest(TRequest request)
        {
            return request.GetType().Name.EndsWith("Query");
        }

        private static string GetCacheKey(TRequest request)
        {
            return request switch
            {
                GetProductByIdQuery query => CacheKeys.Product(query.Id),
                GetAllProductsQuery query => CacheKeys.ProductsList(GetJsonStingValues(JsonSerializer.Serialize(query))),
                _ => string.Empty
            };
        }
        private TimeSpan GetCacheExpiry(TRequest request)
        {
            return request switch
            {
                GetProductByIdQuery => CacheSettings.ProductDetails,
                GetAllProductsQuery => CacheSettings.ProductList,
                _ => CacheSettings.Default
            };
        }

        private static string GetJsonStingValues(string jsonString)
        {
            using JsonDocument doc = JsonDocument.Parse(jsonString);

            string result = "";
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                if (!string.IsNullOrEmpty(result))
                    result += ":";
                result += $"{property.Name}_{property.Value}";
            }
            return result;
        }
    }

}
