namespace IdentityService.Services;

public interface IRoleService
{
    Task<bool> CreateRoleAsync(string name, string description, CancellationToken ct);
    Task<bool> DeleteRoleAsync(string roleName, CancellationToken ct);
    Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct);
    Task<bool> UnassignRoleAsync(string userId, string roleName, CancellationToken ct);
    Task<IEnumerable<string>> GetUsersWithRoleAsync(string requestName, CancellationToken contextCancellationToken);
    Task<IEnumerable<RoleSummary>> GetRolesAsync(CancellationToken ct);
    Task<IEnumerable<UserWithRoles>> GetUsersWithRolesAsync(CancellationToken ct);
}

public class RoleService : IRoleService
{
    private readonly IKeycloakService _keycloakService;
    private readonly IValidationService _validationService;

    public RoleService(IKeycloakService keycloakService, IValidationService validationService)
    {
        _keycloakService = keycloakService;
        _validationService = validationService;
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
        var result = await _keycloakService.AssignRoleAsync(userId, roleName, ct);
        if (result)
        {
            await _validationService.InvalidateUserRolesCacheAsync(userId);
        }
        return result;
    }

    public async Task<bool> UnassignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        var result = await _keycloakService.UnassignRoleAsync(userId, roleName, ct);
        if (result)
        {
            await _validationService.InvalidateUserRolesCacheAsync(userId);
        }
        return result;
    }

    public async Task<IEnumerable<string>> GetUsersWithRoleAsync(string requestName, CancellationToken contextCancellationToken)
    {
        var role = await _keycloakService.GetRoleByNameAsync(requestName, contextCancellationToken);
        if (role == null)
        {
            return Enumerable.Empty<string>();
        }

        var usersWithRoles = await _keycloakService.GetUsersWithRoleAsync(requestName, contextCancellationToken);
        return usersWithRoles.Select(user => user.Id).ToList();
    }

    public async Task<IEnumerable<RoleSummary>> GetRolesAsync(CancellationToken ct)
    {
        var roles = await _keycloakService.GetRolesAsync(ct);
        return roles.Select(role => new RoleSummary(role.Name ?? string.Empty, role.Description ?? string.Empty));
    }

    public async Task<IEnumerable<UserWithRoles>> GetUsersWithRolesAsync(CancellationToken ct)
    {
        var users = await _keycloakService.GetUsersAsync(ct);
        var results = new List<UserWithRoles>();

        foreach (var user in users)
        {
            if (string.IsNullOrWhiteSpace(user.Id))
            {
                continue;
            }

            var roles = await _keycloakService.GetRolesForUserAsync(user.Id, ct);
            results.Add(new UserWithRoles(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                roles.Select(role => role.Name ?? string.Empty)));
        }

        return results;
    }
}

public record RoleSummary(string Name, string Description);

public record UserWithRoles(string UserId, string Email, string FirstName, string LastName, IEnumerable<string> Roles);
