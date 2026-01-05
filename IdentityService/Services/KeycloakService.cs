using System.Net;
using AuthTools.Models;
using Flurl.Http;
using IdentityService.Dtos;
using Keycloak.Net;
using Keycloak.Net.Core.Models.Root;
using Keycloak.Net.Models.Roles;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Keycloak.Net.Models.Clients;

namespace IdentityService.Services;

public interface IKeycloakService
{
    Task<string?> RegisterAsync(RegisterDto registerDto, CancellationToken ct);
    Task<Token> AuthenticateAsync(string email, string password, CancellationToken ct);
    Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct, bool forClient = true);
    Task<bool> UnassignRoleAsync(string userId, string roleName, CancellationToken ct);
    Task<bool> CreateRoleAsync(string name, string description, CancellationToken ct);
    Task<bool> DeleteRoleAsync(string roleName, CancellationToken ct);
    Task<IEnumerable<Role>> GetRolesForUserAsync(string userId, CancellationToken ct);
    Task<Role?> GetRoleByNameAsync(string name, CancellationToken ct, bool forClient = true);
    Task<IEnumerable<User>> GetUsersWithRoleAsync(string requestName, CancellationToken ct);
    Task<IEnumerable<Role>> GetRolesAsync(CancellationToken ct);
    Task<IEnumerable<User>> GetUsersAsync(CancellationToken ct);
}

public class KeycloakService : IKeycloakService
{
    private const string ClientCacheKey = "keycloak_client";
    private readonly KeycloakClient _client;
    private readonly string _realmName;
    private readonly string _clientId;
    private readonly string _secret;
    private readonly IMemoryCache _cache;
    private const string RoleCacheKeyPrefix = "role_";
    private static readonly TimeSpan RoleCacheDuration = TimeSpan.FromMinutes(30);

    public KeycloakService(IOptions<KeycloakConfigurationOptions> options, IMemoryCache cache)
    {
        _client = new KeycloakClient(options.Value.ServerUrl, options.Value.AdminUsername, options.Value.AdminPassword);
        _realmName = options.Value.Realm;
        _clientId = options.Value.ClientId;
        _secret = options.Value.Secret;
        _cache = cache;
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

    public async Task<bool> AssignRoleAsync(string userId, string roleName, CancellationToken ct, bool forClient = true)
    {
        try
        {
            var roleFound = await GetRoleByNameAsync(roleName, ct, forClient);
            if (roleFound == null)
            {
                return false;
            }

            var client = await GetClientAsync(ct);
            return forClient
                ? await _client.AddClientRoleMappingsToUserAsync(_realmName, userId, client.Id, [roleFound], ct)
                : await _client.AddRealmRoleMappingsToUserAsync(_realmName, userId, [roleFound], ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to assign role '{roleName}' to user '{userId}'", ex);
        }
    }

    public async Task<bool> UnassignRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        try
        {
            var roleFound = await GetRoleByNameAsync(roleName, ct);
            if (roleFound == null)
            {
                return false;
            }

            return await _client.DeleteRealmRoleMappingsFromUserAsync(_realmName, userId, [roleFound], ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to unassign role '{roleName}' from user '{userId}'", ex);
        }
    }

    public async Task<bool> CreateRoleAsync(string name, string description, CancellationToken ct)
    {
        var client = await GetClientAsync(ct);
        return await _client.CreateRoleAsync(_realmName, client.Id, new Role
        {
            Name = name,
            Description = description,
            Composite = false,
            ClientRole = true,
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

    public async Task<Role?> GetRoleByNameAsync(string name, CancellationToken ct, bool forClient = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var cacheKey = $"{RoleCacheKeyPrefix}{name}";
        if (_cache.TryGetValue(cacheKey, out Role? cachedRole))
        {
            return cachedRole;
        }

        try
        {
            var client = await GetClientAsync(ct);
            var role = forClient
                ? await _client.GetRoleByNameAsync(_realmName, client.Id, name, ct)
                : await _client.GetRoleByNameAsync(_realmName, name, ct);
            if (role == null)
            {
                return role;
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(RoleCacheDuration);
            _cache.Set(cacheKey, role, cacheOptions);
            return role;
        }
        catch (FlurlHttpException ex) when (ex.StatusCode == (int)HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (FlurlHttpException ex)
        {
            throw new InvalidOperationException($"Failed to get role. Status code: {ex.StatusCode}", ex);
        }
    }

    public async Task<IEnumerable<User>> GetUsersWithRoleAsync(string name, CancellationToken ct)
    {
        var role = await GetRoleByNameAsync(name, ct);
        if (role == null)
        {
            return [];
        }

        const int pageSize = 100;
        var allUsers = new List<User>();
        var currentFirst = 0;

        while (true)
        {
            var users = await _client.GetUsersWithRoleNameAsync(
                _realmName,
                name,
                first: currentFirst,
                max: pageSize,
                ct);

            var usersList = users.ToList();
            if (usersList.Count == 0)
                break;

            allUsers.AddRange(usersList);
            currentFirst += pageSize;

            await Task.Delay(100, ct);
        }

        return allUsers;
    }

    public async Task<IEnumerable<Role>> GetRolesAsync(CancellationToken ct)
    {
        var client = await GetClientAsync(ct);
        const int pageSize = 100;
        var allRoles = new List<Role>();
        var currentFirst = 0;

        while (true)
        {
            var roles = await _client.GetRolesAsync(_realmName, client.Id, currentFirst, pageSize, search: null, ct);
            var rolesList = roles?.ToList() ?? new List<Role>();
            if (rolesList.Count == 0)
            {
                break;
            }

            allRoles.AddRange(rolesList);
            currentFirst += pageSize;
        }

        return allRoles;
    }

    public async Task<IEnumerable<User>> GetUsersAsync(CancellationToken ct)
    {
        const int pageSize = 100;
        var allUsers = new List<User>();
        var currentFirst = 0;

        while (true)
        {
            var users = await _client.GetUsersAsync(
                _realmName,
                briefRepresentation: true,
                email: null,
                emailVerified: null,
                enabled: null,
                exact: null,
                first: currentFirst,
                firstName: null,
                idpAlias: null,
                idpUserId: null,
                lastName: null,
                max: pageSize,
                q: null,
                search: null,
                username: null,
                cancellationToken: ct);
            var usersList = users?.ToList() ?? new List<User>();
            if (usersList.Count == 0)
            {
                break;
            }

            allUsers.AddRange(usersList);
            currentFirst += pageSize;
        }

        return allUsers;
    }

    private async Task<Client?> GetClientAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(ClientCacheKey, out Client? cachedClient))
        {
            return cachedClient;
        }

        var client = (await _client.GetClientsAsync(_realmName, q: _clientId, cancellationToken: ct)).First();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(RoleCacheDuration);
        _cache.Set(ClientCacheKey, client, cacheOptions);

        return client;
    }
}
