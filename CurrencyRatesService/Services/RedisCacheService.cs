using StackExchange.Redis;

namespace CurrencyRatesService.Services;

public interface IRedisCacheService
{
    Task SetAsync(string key, object? value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task DeleteAsync(string key);
    Task<bool> HasKeyAsync(string key);
}

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(string connectionString)
    {
        _db = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
    }

    public async Task SetAsync(string key, object? value, TimeSpan? expiry = null)
    {
        var serializedValue = RedisJsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, serializedValue, expiry);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var stringResult = await _db.StringGetAsync(key);
        var deserializedResult = RedisJsonSerializer.Deserialize<T>(stringResult);
        return deserializedResult;
    }

    public async Task DeleteAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> HasKeyAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }
}