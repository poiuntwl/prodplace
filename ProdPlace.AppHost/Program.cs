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
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HOSTNAME", "localhost")
    .WithReference(keycloakDb);

// Services
var identityService = builder.AddProject<Projects.IdentityService>("identity-service")
    .WithReference(identityDb)
    .WithHttpsEndpoint(port: 44304, name: "https")
    .WithExternalHttpEndpoints();

var productsService = builder.AddProject<Projects.ProductsService>("products-service")
    .WithReference(productsDb)
    .WithReference(mongodb)
    .WithReference(rabbitMq)
    .WithReference(identityService)
    .WithHttpsEndpoint(port: 44301, name: "https")
    .WithExternalHttpEndpoints();

var priceService = builder.AddProject<Projects.PriceService>("price-service")
    .WithHttpsEndpoint(port: 44302, name: "https")
    .WithExternalHttpEndpoints();

var currencyRatesService = builder.AddProject<Projects.CurrencyRatesService>("currency-rates-service")
    .WithReference(currenciesDb)
    .WithReference(redis)
    .WithHttpsEndpoint(port: 44303, name: "https")
    .WithExternalHttpEndpoints();

var orderService = builder.AddProject<Projects.OrderService>("order-service")
    .WithReference(ordersDb)
    .WithHttpsEndpoint(port: 44305, name: "https")
    .WithExternalHttpEndpoints();

var customerService = builder.AddProject<Projects.CustomerService>("customer-service")
    .WithReference(customersDb)
    .WithHttpsEndpoint(port: 44306, name: "https")
    .WithExternalHttpEndpoints();

var adminService = builder.AddProject<Projects.Prodplace_Admin>("admin-service")
    .WithReference(identityService)
    .WithReference(currencyRatesService)
    .WithHttpsEndpoint(port: 44307, name: "https")
    .WithExternalHttpEndpoints();

var proxyService = builder.AddProject<Projects.ProxyService>("proxy-service")
    .WithHttpEndpoint(port: 44300, name: "http")
    .WithExternalHttpEndpoints();

// Frontend applications
var backoffice = builder.AddNpmApp("backoffice", "../backoffice-web")
    .WithHttpEndpoint(port: 12346, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

var frontend = builder.AddNpmApp("frontend", "../fe")
    .WithHttpEndpoint(port: 8080, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
