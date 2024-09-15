using Newtonsoft.Json;
using StackExchange.Redis;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CurrencyRatesService.Services;

public static class RedisJsonSerializer
{
    public static RedisValue Serialize(object? value)
    {
        if (value == null)
        {
            return RedisValue.Null;
        }

        return JsonConvert.SerializeObject(value);
    }

    public static T? Deserialize<T>(RedisValue value)
    {
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        var str = value.ToString();
        return string.IsNullOrWhiteSpace(str) ? default : JsonSerializer.Deserialize<T>(str);
    }
}