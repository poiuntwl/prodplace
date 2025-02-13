using Keycloak.AuthServices.Authentication;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddKeycloakWebApiAuthentication(builder.Configuration);
services.AddEndpointsApiExplorer();
services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
services.AddCors(x => x.AddPolicy("VueCorsPolicy", y =>
    y.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("http://localhost:12345")));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("VueCorsPolicy");

app.MapReverseProxy();

app.UseHttpsRedirection();
app.Run();