using AuthTools.Constants;
using IdentityService.Dtos;
using IdentityService.Extensions;
using IdentityService.Models;
using Keycloak.Net;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Options;

namespace IdentityService.Services;

public interface IKeycloakService
{
    Task<string?> RegisterAsync(RegisterDto registerDto, CancellationToken ct);
    Task AssignRoleAsync(string userId, UserRole role, CancellationToken ct);
}

public class KeycloakService : IKeycloakService
{
    private readonly KeycloakClient _client;
    private readonly string _realmName;

    public KeycloakService(IOptions<KeycloakConfiguration> options)
    {
        _client = new KeycloakClient(options.Value.ServerUrl, options.Value.AdminUsername, options.Value.AdminPassword);
        _realmName = options.Value.Realm;
    }

    public async Task<string?> RegisterAsync(RegisterDto registerDto, CancellationToken ct)
    {
        var user = new User
        {
            UserName = registerDto.Username,
            Email = registerDto.Email,
            Enabled = true,
            Credentials =
            [
                new Credentials
                {
                    Type = "password",
                    Value = registerDto.Password,
                    Temporary = false
                }
            ]
        };
        var userId = await _client.CreateAndRetrieveUserIdAsync(_realmName, user, ct);

        if (userId == null)
        {
            return userId;
        }

        try
        {
            var userRole = await _client.GetRoleByNameAsync(_realmName, UserRole.User.GetDescription(), ct);
            await _client.AddRealmRoleMappingsToUserAsync(_realmName, userId, [userRole], ct);
        }
        catch (Exception)
        {
            return null;
        }


        return userId;
    }

    public async Task AssignRoleAsync(string userId, UserRole role, CancellationToken ct)
    {
        var roleFound = await _client.GetRoleByNameAsync(_realmName, role.GetDescription(), ct);
        await _client.AddRealmRoleMappingsToUserAsync(_realmName, userId, [roleFound], ct);
    }
}