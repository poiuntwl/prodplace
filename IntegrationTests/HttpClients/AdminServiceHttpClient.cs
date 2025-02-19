extern alias AdminSUT;
using System.Net.Http.Json;
using System.Text.Json;
using AdminSUT::Prodplace.Admin;

namespace IntegrationTests.HttpClients;

extern alias AdminSUT;

public interface IAdminServiceHttpClient : IDisposable
{
    Task<CreateRoleDto?> CreateRole(CreateRoleRequestDto role);
    Task<AssignRoleDto?> AssignRole(AssignRoleRequestDto dto);
}

public class AdminServiceHttpClient : IAdminServiceHttpClient
{
    private readonly HttpClient _httpClient;

    public AdminServiceHttpClient(HttpClient client)
    {
        _httpClient = client;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<CreateRoleDto?> CreateRole(CreateRoleRequestDto role)
    {
        var response = await _httpClient.PostAsJsonAsync("/roles", role);
        return await response.Content.ReadFromJsonAsync<CreateRoleDto>();
    }

    public async Task<AssignRoleDto?> AssignRole(AssignRoleRequestDto dto)
    {
        var response = await _httpClient.SendRequestAsync("/roles/assign", HttpMethod.Post, dto);
        return JsonSerializer.Deserialize<AssignRoleDto>(response, new JsonSerializerOptions
               {
                   PropertyNameCaseInsensitive = true
               })
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}