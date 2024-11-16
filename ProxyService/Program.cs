using System.Security.Claims;
using System.Text.Json.Serialization;
using AuthTools;
using AuthTools.Services;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var s = builder.Services;
s.AddJwtAuthConfiguration(builder.Configuration);
s.AddJwtAuthServices();

s.AddEndpointsApiExplorer();
s.AddSwaggerGen();
s.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

s.AddCors(x =>
{
    x.AddPolicy("VueCorsPolicy",
        y => { y.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:12345"); });
});

var app = builder.Build();
app.UseCors("VueCorsPolicy");

app.MapPost("/login",
    async (LoginRequest creds, IKeycloakHttpClient keycloakHttpClient, CancellationToken ct) =>
    {
        var accessToken = await keycloakHttpClient.GetAccessTokenAsync(creds.Username, creds.Password, ct);

        return Results.Ok(accessToken);
    });

app.MapPost("/refresh",
    async (RefreshRequest request, IKeycloakHttpClient keycloakHttpClient, CancellationToken ct) =>
    {
        var accessToken = await keycloakHttpClient.RefreshAsync(request.RefreshToken, ct);

        return Results.Ok(accessToken);
    });

app.MapReverseProxy();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapGet("/hello", () => "Hello World!");

app.Run();

internal record LoginRequest(string Username, string Password);

internal record RefreshRequest
{
    public RefreshRequest(string RefreshToken)
    {
        this.RefreshToken = RefreshToken;
    }

    [JsonPropertyName("refresh_token")] public string RefreshToken { get; init; }

    public void Deconstruct(out string RefreshToken)
    {
        RefreshToken = this.RefreshToken;
    }
}