using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CubArt.Infrastructure.Caching
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task RemoveByPatternAsync(string pattern);
    }

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = redis.GetDatabase();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue) return default;

            return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.StringSetAsync(key, serializedValue, expiry);
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());
            var keys = server.Keys(pattern: pattern).ToArray();

            foreach (var key in keys)
            {
                await _database.KeyDeleteAsync(key);
            }
        }
    }

}
