using IdentityService.Dtos;

namespace IntegrationTests.HttpClients;

public interface IIdentityServiceHttpClient : IDisposable
{
    Task<UserDataResult?> Register(RegisterDto dto);
    Task<UserDataResult?> Login(LoginDto dto, string jwt);
}

public class IdentityServiceHttpClient : IIdentityServiceHttpClient
{
    private readonly HttpClient _httpClient;

    public IdentityServiceHttpClient(HttpClient client)
    {
        _httpClient = client;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<UserDataResult?> Register(RegisterDto dto)
    {
        return await _httpClient.SendRequestAsync<UserDataResult?>("api/account/register", HttpMethod.Post, dto);
    }

    public async Task<UserDataResult?> Login(LoginDto dto, string jwt)
    {
        return await _httpClient.SendRequestAsync<UserDataResult?>("api/account/login", HttpMethod.Post, jwt: jwt);
    }
}