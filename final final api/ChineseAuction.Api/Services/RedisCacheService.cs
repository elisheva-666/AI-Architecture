using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ChineseAuction.Api.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var json = await _cache.GetStringAsync(key);
                if (json is null) return default;

                _logger.LogInformation("Cache HIT: {Key}", key);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                // Redis unavailable — fall through to the database
                _logger.LogWarning(ex, "Cache GET failed for key {Key}. Falling back to database.", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                });
                _logger.LogInformation("Cache SET: {Key} (TTL {TTL})", key, ttl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache SET failed for key {Key}.", key);
            }
        }

        public async Task RemoveAsync(params string[] keys)
        {
            foreach (var key in keys)
            {
                try
                {
                    await _cache.RemoveAsync(key);
                    _logger.LogInformation("Cache INVALIDATED: {Key}", key);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cache REMOVE failed for key {Key}.", key);
                }
            }
        }
    }
}
