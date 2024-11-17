using System.Text.Json.Serialization;
using AuthTools;
using AuthTools.Services;
using ProxyService;

var builder = WebApplication.CreateBuilder(args);

var s = builder.Services;
s.AddJwtAuthConfiguration(builder);
s.AddJwtAuthServices();
s.AddSingleton<IGatewayService, GatewayService>();

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
app.UseJwtAuthConfiguration();
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
        var accessToken = await keycloakHttpClient.RefreshTokenAsync(request.RefreshToken, ct);

        return Results.Ok(accessToken);
    });

app.MapGet("/hello", () => "Hello World!").RequireAuthorization();
app.MapGet("/hello-insecure", () => "Hello World!");

app.MapReverseProxy(x =>
{
    var gatewayService = x.ApplicationServices.GetRequiredService<IGatewayService>();
    x.Use(async (ctx, next) =>
    {
        await gatewayService.AddTokenAsync(ctx, ctx.RequestAborted);
        await next().ConfigureAwait(false);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

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