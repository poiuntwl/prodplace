using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.BearerToken;

var builder = WebApplication.CreateBuilder(args);

var s = builder.Services;

s.AddEndpointsApiExplorer();
s.AddSwaggerGen();
s.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

s.AddAuthentication(BearerTokenDefaults.AuthenticationScheme)
    .AddBearerToken();

var app = builder.Build();
app.MapGet("login", () => Results.SignIn(new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("sub", Guid.NewGuid().ToString())
        ],
        BearerTokenDefaults.AuthenticationScheme)),
    authenticationScheme: BearerTokenDefaults.AuthenticationScheme));

app.UseAuthentication().UseAuthorization();
app.MapReverseProxy();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapGet("/hello", () => "Hello World!");

app.Run();