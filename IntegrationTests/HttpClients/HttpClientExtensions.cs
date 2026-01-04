using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace IntegrationTests.HttpClients;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<string> SendRequestAsync(this HttpClient httpClient, string url,
        HttpMethod? httpMethod = null, object? body = null, string? jwt = null)
    {
        using var httpRequestMessage = new HttpRequestMessage(httpMethod ?? HttpMethod.Get, url);

        if (string.IsNullOrWhiteSpace(jwt) == false)
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        if (body != null)
            httpRequestMessage.Content =
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, MediaTypeNames.Application.Json);

        using var response = await httpClient.SendAsync(httpRequestMessage);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Request to {url} failed with status {(int)response.StatusCode} ({response.ReasonPhrase}). Response: {responseContent}");
        }
        
        return responseContent;
    }

    public static async Task<T?> SendRequestAsync<T>(this HttpClient httpClient, string url,
        HttpMethod? httpMethod = null, object? body = null, string? jwt = null)
    {
        var responseJson = await SendRequestAsync(httpClient, url, httpMethod, body, jwt);
        return JsonSerializer.Deserialize<T>(responseJson, JsonSerializerOptions);
    }
}