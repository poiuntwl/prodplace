using System.Text.Json;

namespace ProductsService.Services;

public class AuthHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AuthHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["Auth:IdentityServiceConnectionString"]!;
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/auth/validate-token?token={token}", ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ValidateRolesAsync(string token, string[] roles, CancellationToken ct)
    {
        var rolesString = string.Join(",", roles);
        var response =
            await _httpClient.GetAsync(
                $"{_baseUrl}/api/auth/validate-roles?token={token}&roles={Uri.EscapeDataString(rolesString)}",
                ct);
        return response.IsSuccessStatusCode;
    }
}