using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuthTools.Services;

public interface IKeycloakHttpClient
{
    Task<TokenDataModel?> GetAccessTokenAsync(string username, string password, CancellationToken ct);
    Task<TokenDataModel?> RefreshTokenAsync(string refreshToken, CancellationToken ct);
}

public class KeycloakHttpClient : IKeycloakHttpClient
{
    private const string ClientSecret = "hWsjheX4uUAEKrvQxDT5KQvCjlVnk1fZ";
    private readonly HttpClient _httpClient;

    public KeycloakHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://127.0.0.1:8080/realms/prodplace/");
    }

    public async Task<TokenDataModel?> GetAccessTokenAsync(string username, string password, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "protocol/openid-connect/token");
        // todo: move to configuration later
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", "jwt" },
            { "grant_type", "password" },
            { "client_secret", ClientSecret },
            { "username", username },
            { "password", password },
        });
        using var response = await _httpClient.SendAsync(request, ct);
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<GetAccessTokenResponse>(responseJson);

        return result?.ToTokenDataModel();
    }

    public async Task<TokenDataModel?> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "protocol/openid-connect/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", "jwt" },
            { "grant_type", "refresh_token" },
            { "client_secret", ClientSecret }, // todo: move to configuration later
            { "refresh_token", refreshToken },
        });
        using var response = await _httpClient.SendAsync(request, ct);
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<GetAccessTokenResponse>(responseJson);

        return result?.ToTokenDataModel();
    }
}

public class GetAccessTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; }
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public int? RefreshExpiresIn { get; set; }
}

public class TokenDataModel
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }
    [JsonPropertyName("token_due")] public DateTimeOffset? TokenDue { get; set; }
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }
    [JsonPropertyName("refresh_due")] public DateTimeOffset? RefreshDue { get; set; }
}

public static class TokenDataModelMapExtensions
{
    public static TokenDataModel ToTokenDataModel(this GetAccessTokenResponse response)
    {
        return new TokenDataModel
        {
            AccessToken = response.AccessToken,
            TokenDue = response.ExpiresIn != null
                ? new DateTimeOffset(DateTime.UtcNow.AddSeconds(response.ExpiresIn.Value), TimeSpan.Zero)
                : null,
            RefreshToken = response.RefreshToken,
            RefreshDue = response.RefreshExpiresIn != null
                ? new DateTimeOffset(DateTime.UtcNow.AddSeconds(response.RefreshExpiresIn.Value), TimeSpan.Zero)
                : null
        };
    }
}