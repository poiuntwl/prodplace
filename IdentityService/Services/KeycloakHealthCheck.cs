using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IdentityService.Services;

public class KeycloakHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public KeycloakHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient("keycloak-health");
            var response = await client.GetAsync("health/live", cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Keycloak is responding")
                : HealthCheckResult.Unhealthy("Keycloak health check failed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Unable to reach Keycloak server", ex);
        }
    }
}
