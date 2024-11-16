using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Azure.Identity;

namespace IdentityService.Services;

public interface IKeycloakHttpClient
{
}

public class KeycloakHttpClient : IKeycloakHttpClient
{
    private readonly HttpClient _httpClient;

    public KeycloakHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task GetAccessTokenAsync(CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("protocol/openid-connect/token"));
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            client_id = "public",
            grant_type = "client_credentials",
            client_secret = "AxWN4GSgjVds1Fr2SDrzwK2mLWgzuB66"
        }), Encoding.UTF8, MediaTypeNames.Application.Json);
        using var response = await _httpClient.SendAsync(request, ct);
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<dynamic>(responseJson);

        return result;
    }
}