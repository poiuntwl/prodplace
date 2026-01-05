var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure - Databases
var sqlPassword = builder.AddParameter("sql-password", secret: true);

var productsDb = builder.AddSqlServer("sql-server-products", password: sqlPassword, port: 5400)
    .AddDatabase("products-db");

var currenciesDb = builder.AddSqlServer("sql-server-currencies", password: sqlPassword, port: 5401)
    .AddDatabase("currencies-db");

var identityDb = builder.AddSqlServer("sql-server-identity", password: sqlPassword, port: 5402)
    .AddDatabase("identity-db");

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var ordersDb = builder.AddPostgres("postgres-orders", password: postgresPassword, port: 5403)
    .WithPgAdmin()
    .AddDatabase("orders-db");

var customersDb = builder.AddPostgres("postgres-customers", password: postgresPassword, port: 5404)
    .AddDatabase("customers-db");

var keycloakDb = builder.AddPostgres("postgres-keycloak", password: postgresPassword)
    .AddDatabase("keycloak-db", "keycloak_db");

// Infrastructure - Messaging & Caching
var mongodb = builder.AddMongoDB("mongodb", port: 27017)
    .WithMongoExpress()
    .AddDatabase("mongo-db");

var redis = builder.AddRedis("redis-cache", port: 6379);

var rabbitMq = builder.AddRabbitMQ("rbmq", port: 5472);

// Keycloak (custom container)
var keycloak = builder.AddContainer("keycloak", "prodplace-keycloak-prebuilt", "latest")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HOSTNAME", "localhost")
    .WithReference(keycloakDb);

// Services
var identityService = builder.AddProject<Projects.IdentityService>("identity-service")
    .WithReference(identityDb)
    .WithExternalHttpEndpoints();

var productsService = builder.AddProject<Projects.ProductsService>("products-service")
    .WithReference(productsDb)
    .WithReference(mongodb)
    .WithReference(rabbitMq)
    .WithReference(identityService)
    .WithExternalHttpEndpoints();

var priceService = builder.AddProject<Projects.PriceService>("price-service")
    .WithExternalHttpEndpoints();

var currencyRatesService = builder.AddProject<Projects.CurrencyRatesService>("currency-rates-service")
    .WithReference(currenciesDb)
    .WithReference(redis)
    .WithExternalHttpEndpoints();

var orderService = builder.AddProject<Projects.OrderService>("order-service")
    .WithReference(ordersDb)
    .WithExternalHttpEndpoints();

var customerService = builder.AddProject<Projects.CustomerService>("customer-service")
    .WithReference(customersDb)
    .WithExternalHttpEndpoints();

var adminService = builder.AddProject<Projects.Prodplace_Admin>("admin-service")
    .WithReference(identityService)
    .WithReference(currencyRatesService)
    .WithExternalHttpEndpoints();

var proxyService = builder.AddProject<Projects.ProxyService>("proxy-service")
    .WithExternalHttpEndpoints();

// Frontend applications
var backoffice = builder.AddNpmApp("backoffice", "../backoffice-web")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

var frontend = builder.AddNpmApp("frontend", "../fe")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
