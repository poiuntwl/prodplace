namespace MessagingTools;

public class RabbitMqSettings
{
    public string QueueName { get; set; }
    public string HostName { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    private string? _connectionString;

    public string ConnectionString
    {
        get => _connectionString ?? GenerateConnectionString();
        set => _connectionString = value;
    }

    public string GenerateConnectionString()
    {
        return $"amqp://{Uri.EscapeDataString(UserName)}:{Uri.EscapeDataString(Password)}@{HostName}:{Port}";
    }
}