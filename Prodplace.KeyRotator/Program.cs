using Prodplace.KeyRotator;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<KeyRotationService>();

var host = builder.Build();
host.Run();