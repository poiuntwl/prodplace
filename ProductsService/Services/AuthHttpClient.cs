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
        var response = await _httpClient.GetAsync($"{_baseUrl}/validate?token={token}", ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ValidateRolesAsync(string token, string[] roles, CancellationToken ct)
    {
        var rolesJson = JsonSerializer.Serialize(roles);
        var response =
            await _httpClient.GetAsync(
                $"{_baseUrl}/validate-roles?token={token}&roles={Uri.EscapeDataString(rolesJson)}",
                ct);
        return response.IsSuccessStatusCode;
    }
}