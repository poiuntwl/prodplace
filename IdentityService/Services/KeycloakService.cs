using AuthTools.Models;
using IdentityService.Dtos;
using Keycloak.Net;
using Keycloak.Net.Core.Models.Root;
using Keycloak.Net.Models.Roles;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Options;
using KeycloakOptions = AuthTools.Models.KeycloakOptions;

namespace IdentityService.Services;

public interface IKeycloakService
{
    Task<string?> RegisterAsync(RegisterDto registerDto, CancellationToken ct);
    Task<Token> AuthenticateAsync(string email, string password, CancellationToken ct);
    Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct);
    Task<bool> UnassignRoleAsync(string userId, string roleName, CancellationToken ct);
    Task<bool> CreateRoleAsync(string name, string description, CancellationToken ct);
    Task<bool> DeleteRoleAsync(string roleName, CancellationToken ct);
    Task<IEnumerable<Role>> GetRolesForUserAsync(string userId, CancellationToken ct);
    Task<Role> GetRoleByNameAsync(string name, CancellationToken ct);
}

public class KeycloakService : IKeycloakService
{
    private readonly KeycloakClient _client;
    private readonly string _realmName;
    private readonly string _clientId;
    private readonly string _secret;

    public KeycloakService(IOptions<KeycloakOptions> options)
    {
        _client = new KeycloakClient(options.Value.ServerUrl, options.Value.AdminUsername, options.Value.AdminPassword);
        _realmName = options.Value.Realm;
        _clientId = options.Value.ClientId;
        _secret = options.Value.Secret;
    }

    public async Task<string?> RegisterAsync(RegisterDto registerDto, CancellationToken ct)
    {
        var user = new User
        {
            Email = registerDto.Email,
            UserName = registerDto.Email.Split("@")[0],
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Enabled = true,
            Credentials =
            [
                new Credentials
                {
                    Type = "password",
                    Value = registerDto.Password,
                    Temporary = false,
                }
            ]
        };
        var userId = await _client.CreateAndRetrieveUserIdAsync(_realmName, user, ct);
        return userId;
    }

    public async Task<Token> AuthenticateAsync(string email, string password, CancellationToken ct)
    {
        var token = await _client.GetTokenWithResourceOwnerPasswordCredentialsAsync(_realmName, _clientId, email,
            password, _secret,
            ct);
        return token;
    }

    public async Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        var roleFound = await GetRoleByNameAsync(_realmName, ct);
        return await _client.AddRealmRoleMappingsToUserAsync(_realmName, userId, [roleFound], ct);
    }

    public async Task<bool> UnassignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        var roleFound = await GetRoleByNameAsync(_realmName, ct);
        return await _client.DeleteRealmRoleMappingsFromUserAsync(_realmName, userId, [roleFound], ct);
    }

    public async Task<bool> CreateRoleAsync(string name, string description, CancellationToken ct)
    {
        return await _client.CreateRoleAsync(_realmName, new Role
        {
            Name = name,
            Description = description,
            Composite = false,
            ClientRole = false,
            ContainerId = _realmName,
        }, ct);
    }

    public async Task<bool> DeleteRoleAsync(string roleName, CancellationToken ct)
    {
        return await _client.DeleteRoleByNameAsync(_realmName, roleName, ct);
    }

    public async Task<IEnumerable<Role>> GetRolesForUserAsync(string userId, CancellationToken ct)
    {
        return await _client.GetEffectiveClientRoleMappingsForUserAsync(_realmName, userId, _clientId, ct);
    }

    public async Task<Role> GetRoleByNameAsync(string name, CancellationToken ct)
    {
        return await _client.GetRoleByNameAsync(_realmName, _clientId, name, ct);
    }
}