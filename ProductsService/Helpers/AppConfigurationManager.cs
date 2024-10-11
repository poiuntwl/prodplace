using ProductsService.Interfaces;

namespace ProductsService.Helpers;

public class AppConfigurationManager : IAppConfigurationManager
{
    public string? DefaultConnectionString { get; set; }
    public string? MongoDefaultConnectionString { get; set; }
    public string? MongoDatabaseName { get; set; }

    public AppConfigurationManager(IConfiguration configuration)
    {
        DefaultConnectionString = configuration.GetConnectionString("DefaultConnection");
        MongoDefaultConnectionString = configuration.GetConnectionString("MongoDefaultConnection");
        MongoDatabaseName = configuration["MongoDb:DatabaseName"];
    }
}