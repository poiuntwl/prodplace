var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure - Databases
var sqlPasswordValue = builder.Configuration["SA_PASSWORD"]
    ?? builder.Configuration["MSSQL_SA_PASSWORD"]
    ?? "Password123!!";
var sqlPassword = builder.AddParameter("sql-password", secret: true, value: sqlPasswordValue);

var productsDb = builder.AddSqlServer("sql-server-products", password: sqlPassword, port: 5400)
    .AddDatabase("products-db");

var currenciesDb = builder.AddSqlServer("sql-server-currencies", password: sqlPassword, port: 5401)
    .AddDatabase("currencies-db");

var identityDb = builder.AddSqlServer("sql-server-identity", password: sqlPassword, port: 5402)
    .AddDatabase("identity-db");

var postgresUserValue = builder.Configuration["POSTGRES_USER"] ?? "admin";
var postgresUser = builder.AddParameter("postgres-user", value: postgresUserValue);
var postgresPasswordValue = builder.Configuration["POSTGRES_PASSWORD"] ?? "Password123!!";
var postgresPassword = builder.AddParameter("postgres-password", secret: true, value: postgresPasswordValue);

var ordersDb = builder.AddPostgres("postgres-orders", password: postgresPassword, port: 5403)
    .WithUserName(postgresUser)
    .WithPgAdmin()
    .AddDatabase("orders-db");

var customersDb = builder.AddPostgres("postgres-customers", password: postgresPassword, port: 5404)
    .WithUserName(postgresUser)
    .AddDatabase("customers-db");

var keycloakDb = builder.AddPostgres("postgres-keycloak", password: postgresPassword)
    .WithUserName(postgresUser)
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
    // Override the baked-in Dockerfile URL so Aspire DNS resolves correctly.
    .WithEnvironment("KC_DB_URL", "jdbc:postgresql://postgres-keycloak:5432/keycloak_db")
    .WithEnvironment("KC_DB_USERNAME", postgresUserValue)
    .WithEnvironment("KC_DB_PASSWORD", postgresPasswordValue)
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
var backoffice = builder.AddBunApp("backoffice", "../backoffice-web", "dev")
    .WithBunPackageInstallation()
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

var frontend = builder.AddBunApp("frontend", "../fe", "dev")
    .WithBunPackageInstallation()
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
