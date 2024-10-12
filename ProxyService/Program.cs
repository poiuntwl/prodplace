using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.BearerToken;

var builder = WebApplication.CreateBuilder(args);

var s = builder.Services;

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

app.MapReverseProxy();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapGet("/hello", () => "Hello World!");

app.Run();