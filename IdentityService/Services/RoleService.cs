namespace IdentityService.Services;

public interface IRoleService
{
    Task<bool> CreateRoleAsync(string name, string description, CancellationToken ct);
    Task<bool> DeleteRoleAsync(string roleName, CancellationToken ct);
    Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct);
    Task<bool> UnassignRoleAsync(string userId, string roleName, CancellationToken ct);
}

public class RoleService : IRoleService
{
    private readonly IKeycloakService _keycloakService;

    public RoleService(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<bool> CreateRoleAsync(string name, string description, CancellationToken ct)
    {
        return await _keycloakService.CreateRoleAsync(name, description, ct);
    }

    public async Task<bool> DeleteRoleAsync(string roleName, CancellationToken ct)
    {
        return await _keycloakService.DeleteRoleAsync(roleName, ct);
    }

    public async Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        return await _keycloakService.AssignRoleAsync(userId, roleName, ct);
    }

    public async Task<bool> UnassignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        return await _keycloakService.UnassignRoleAsync(userId, roleName, ct);
    }
}