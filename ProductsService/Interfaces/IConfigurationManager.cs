namespace ProductsService.Interfaces;

public interface IAppConfigurationManager
{
    public string? DefaultConnectionString { get; set; }
    public string? MongoDefaultConnectionString { get; set; }
    public string? MongoDatabaseName { get; set; }
}