namespace ProductsService.Interfaces;

public interface IAppConfigurationManager
{
    public string? DefaultConnectionString { get; set; }
    public string? MongoDefaultConnectionString { get; set; }
    public string? MongoDatabaseName { get; set; }
    public string? RabbitMqDefaultQueueName { get; set; }
    public string? RabbitMqHostName { get; set; }
    public string? RabbitMqUserName { get; set; }
    public string? RabbitMqPassword { get; set; }
    public int RabbitMqPort { get; set; }
}