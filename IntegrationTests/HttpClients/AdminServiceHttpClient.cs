using System.Net.Http.Json;

namespace IntegrationTests.HttpClients;

extern alias AdminSUT;

public interface IAdminServiceHttpClient : IDisposable
{
    Task<AdminSUT::Prodplace.Admin.CreateRoleDto?> CreateRole(string roleName);
    Task<AdminSUT::Prodplace.Admin.AssignRoleDto?> AssignRole(AdminSUT::Prodplace.Admin.AssignRoleRequestDto dto);
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

    public async Task<AdminSUT::Prodplace.Admin.CreateRoleDto?> CreateRole(string roleName)
    {
        var response = await _httpClient.PostAsJsonAsync("/roles", roleName);
        return await response.Content.ReadFromJsonAsync<AdminSUT::Prodplace.Admin.CreateRoleDto>();
    }

    public async Task<AdminSUT::Prodplace.Admin.AssignRoleDto?> AssignRole(
        AdminSUT::Prodplace.Admin.AssignRoleRequestDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("/roles/assign", dto);
        return await response.Content.ReadFromJsonAsync<AdminSUT::Prodplace.Admin.AssignRoleDto>();
    }
}