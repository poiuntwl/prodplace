using System.Text.Json.Serialization;
using AuthTools;
using AuthTools.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddJwtAuthConfiguration(builder);
services.AddJwtAuthServices();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
services.AddCors(x => x.AddPolicy("VueCorsPolicy", y =>
    y.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("http://localhost:12345")));

var app = builder.Build();

app.UseJwtAuthConfiguration();
app.UseCors("VueCorsPolicy");

app.MapPost("/login", async (LoginRequest creds, CancellationToken ct) =>
    Results.Ok(/*await client.GetAccessTokenAsync(creds.Email, creds.Password, ct)*/));

app.MapPost("/refresh", async (RefreshRequest request, CancellationToken ct) =>
    Results.Ok(/*await client.RefreshTokenAsync(request.RefreshToken, ct)*/));

app.MapGet("/hello", () => "Hello World!").RequireAuthorization();
app.MapGet("/hello-insecure", () => "Hello World!");

app.MapReverseProxy();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();

internal record LoginRequest(string Email, string Password);

internal record RefreshRequest(
    [property: JsonPropertyName("refresh_token")]
    string RefreshToken);