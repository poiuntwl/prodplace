using System.Text;
using System.Text.Json;
using IdentityService.Dtos;

namespace IdentityServiceTests;

[Collection(nameof(IdentityServiceCollectionDefinition))]
public class RegisterUserFixture : IAsyncLifetime
{
    public UserDataResult? User { get; set; }
    public RegisterDto Dto { get; set; }

    private readonly HttpClient _httpClient;

    public RegisterUserFixture(IdentityServiceFactory identityServiceFactory)
    {
        _httpClient = identityServiceFactory.HttpClient;
    }

    private static RegisterDto GenerateRegisterDto()
    {
        var registerBody = new RegisterDto
        {
            Email = "someemail@gmail.com",
            Password = "Somevalidpassword1!",
            Username = "some_username"
        };

        return registerBody;
    }

    public async Task InitializeAsync()
    {
        Dto = GenerateRegisterDto();
        User = await RegisterUserAsync(Dto);
    }

    public async Task DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _httpClient.Dispose();
    }

    private async Task<UserDataResult?> RegisterUserAsync(RegisterDto dto)
    {
        var jsonBody = JsonSerializer.Serialize(dto);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/account/register");
        request.Content = content;
        using var result = await _httpClient.SendAsync(request);
        var responseJson = await result.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<UserDataResult>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return response;
    }
}