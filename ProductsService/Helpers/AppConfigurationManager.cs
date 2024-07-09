using ProductsService.Interfaces;

namespace ProductsService.Helpers;

public class AppConfigurationManager : IAppConfigurationManager
{
    public string? DefaultConnectionString { get; set; }
    public string? MongoDefaultConnectionString { get; set; }
    public string? MongoDatabaseName { get; set; }
    public string? RabbitMqDefaultQueueName { get; set; }
    public string? RabbitMqHostName { get; set; }
    public string? RabbitMqUserName { get; set; }
    public string? RabbitMqPassword { get; set; }
    public int RabbitMqPort { get; set; }

    public AppConfigurationManager(IConfiguration configuration)
    {
        DefaultConnectionString = configuration.GetConnectionString("DefaultConnection");
        MongoDefaultConnectionString = configuration.GetConnectionString("MongoDefaultConnection");
        MongoDatabaseName = configuration["MongoDb:DatabaseName"];
        RabbitMqDefaultQueueName = configuration["RabbitMQ:DefaultQueueName"];
        RabbitMqHostName = configuration["RabbitMQ:HostName"];
        RabbitMqUserName = configuration["RabbitMQ:UserName"];
        RabbitMqPassword = configuration["RabbitMQ:Password"];
        RabbitMqPort = configuration.GetValue<int?>("RabbitMQ:Port") ?? 5672;
    }
}