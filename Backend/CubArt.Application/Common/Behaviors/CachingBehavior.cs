using CubArt.Application.Common.Models;
using CubArt.Application.Facilities.Queries;
using CubArt.Application.Payments.Queries;
using CubArt.Application.Productions.Queries;
using CubArt.Application.Products.Queries;
using CubArt.Application.Purchases.Commands;
using CubArt.Application.Purchases.Queries;
using CubArt.Application.Suppliers.Queries;
using CubArt.Application.Supplies.Queries;
using CubArt.Infrastructure.Caching;
using MediatR;
using Microsoft.Extensions.Logging;
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
                GetProductListQuery query => CacheKeys.ProductList(GetJsonStingValues(JsonSerializer.Serialize(query))),
                GetProductPagedListQuery query => CacheKeys.ProductsPagedList(GetJsonStingValues(JsonSerializer.Serialize(query))),

                GetSupplierListQuery query => CacheKeys.SupplierList,
                GetFacilityListQuery query => CacheKeys.FacilityList,

                GetPurchaseByIdQuery query => CacheKeys.Purchase(query.Id),
                GetPurchaseListQuery query => CacheKeys.PurchaseList(GetJsonStingValues(JsonSerializer.Serialize(query))),
                GetPurchasePagedListQuery query => CacheKeys.PurchasePagedList(GetJsonStingValues(JsonSerializer.Serialize(query))),

                GetPaymentByIdQuery query => CacheKeys.Payment(query.Id),
                GetPaymentPagedListQuery query => CacheKeys.PaymentPagedList(GetJsonStingValues(JsonSerializer.Serialize(query))),

                GetSupplyByIdQuery query => CacheKeys.Supply(query.Id),
                GetSupplyPagedListQuery query => CacheKeys.SupplyPagedList(GetJsonStingValues(JsonSerializer.Serialize(query))),

                GetProductionByIdQuery query => CacheKeys.Production(query.Id),
                GetProductionPagedListQuery query => CacheKeys.ProductionPagedList(GetJsonStingValues(JsonSerializer.Serialize(query))),

                _ => string.Empty
            };
        }
        private TimeSpan GetCacheExpiry(TRequest request)
        {
            return request switch
            {
                GetProductByIdQuery => CacheSettings.ProductDetails,
                GetProductPagedListQuery => CacheSettings.ProductList,
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
