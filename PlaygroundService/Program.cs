using StackExchange.Redis;

internal class Program
{
    public static void Main()
    {
        const string connectionString = "redis-cache:6379";

        bool result;
        try
        {
            using var redis = ConnectionMultiplexer.Connect(connectionString);
            var db = redis.GetDatabase();
            result = db.Ping().TotalSeconds < 1;
        }
        catch
        {
            result = false;
        }

        Console.WriteLine(result);
    }
}