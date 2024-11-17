using AuthTools.Services;

namespace ProxyService;

public interface IGatewayService
{
    Task AddTokenAsync(HttpContext ctx, CancellationToken ct);
}

public class GatewayService : IGatewayService
{
    private readonly IKeycloakHttpClient _keycloakHttpClient;

    public GatewayService(IKeycloakHttpClient keycloakHttpClient)
    {
        _keycloakHttpClient = keycloakHttpClient;
    }

    public async Task AddTokenAsync(HttpContext ctx, CancellationToken ct)
    {
        var authHeader = ctx.Request.Headers.Authorization;
        var accessToken = await _keycloakHttpClient.GetAccessTokenAsync("", "", ct);
        Console.WriteLine("entered");
    }
}