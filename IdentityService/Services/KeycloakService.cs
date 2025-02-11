using AuthTools.Constants;
using IdentityService.Dtos;
using IdentityService.Extensions;
using IdentityService.Models;
using Keycloak.Net;
using Keycloak.Net.Core.Models.Root;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Options;

namespace IdentityService.Services;

public interface IKeycloakService
{
    Task<string?> RegisterAsync(RegisterDto registerDto, CancellationToken ct);
    Task AssignRoleAsync(string userId, UserRole role, CancellationToken ct);
    Task<Token> AuthenticateAsync(string email, string password, CancellationToken ct);
}

public class KeycloakService : IKeycloakService
{
    private readonly KeycloakClient _client;
    private readonly string _realmName;
    private readonly string _clientId;
    private readonly string _secret;

    public KeycloakService(IOptions<KeycloakConfiguration> options)
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
            UserName = registerDto.Email,
            FirstName = registerDto.Email,
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

    public async Task AssignRoleAsync(string userId, UserRole role, CancellationToken ct)
    {
        var roleFound = await _client.GetRoleByNameAsync(_realmName, role.GetDescription(), ct);
        await _client.AddRealmRoleMappingsToUserAsync(_realmName, userId, [roleFound], ct);
    }

    public async Task<Token> AuthenticateAsync(string email, string password, CancellationToken ct)
    {
        var token = await _client.GetTokenWithResourceOwnerPasswordCredentialsAsync(_realmName, _clientId, email,
            password, _secret,
            ct);
        return token;
    }
}