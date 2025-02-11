using AuthTools.Services;

namespace IdentityService.Services;

public interface IRoleService
{
    Task<object> CreateRoleAsync(string name, string description);
    Task DeleteRoleAsync(string roleId);
    Task AssignRoleAsync(string userId, string roleId);
    Task UnassignRoleAsync(string userId, string roleId);
}

public class RoleService : IRoleService
{
    public RoleService()
    {

    }

    public async Task<object> CreateRoleAsync(string name, string description)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteRoleAsync(string roleId)
    {
        throw new NotImplementedException();
    }

    public async Task AssignRoleAsync(string userId, string roleId)
    {
        throw new NotImplementedException();
    }

    public async Task UnassignRoleAsync(string userId, string roleId)
    {
        throw new NotImplementedException();
    }
}