using AuthTools.Services;
using IdentityService.Extensions;
using Keycloak.Net.Models.Roles;
using Microsoft.Extensions.Caching.Memory;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace IdentityService.Services;

public interface IValidationService
{
    Task<bool> ValidateRolesAsync(string token, string[]? roles, CancellationToken ct);
    Task InvalidateUserRolesCacheAsync(string userId);
}

public class ValidationService : IValidationService
{
    private readonly IJwtClaimsPrincipalGetter _claimsPrincipalGetter;
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<ValidationService> _logger;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefix = "user_roles_";

    public ValidationService(
        IJwtClaimsPrincipalGetter claimsPrincipalGetter, 
        IKeycloakService keycloakService,
        ILogger<ValidationService> logger,
        IMemoryCache cache)
    {
        _claimsPrincipalGetter = claimsPrincipalGetter ?? throw new ArgumentNullException(nameof(claimsPrincipalGetter));
        _keycloakService = keycloakService ?? throw new ArgumentNullException(nameof(keycloakService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<bool> ValidateRolesAsync(string token, string[]? roles, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Token validation failed: Token is null or empty");
            return false;
        }

        if (roles == null || roles.Length == 0)
        {
            _logger.LogWarning("Role validation failed: No roles provided");
            return false;
        }

        try
        {
            var claimsPrincipal = _claimsPrincipalGetter.Get(token);
            if (claimsPrincipal == null)
            {
                _logger.LogWarning("Token validation failed: Unable to get claims principal");
                return false;
            }

            var userId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Token validation failed: Unable to extract user ID from token");
                return false;
            }

            var userRoles = await GetUserRolesFromCacheAsync(userId, ct);
            if (!userRoles.Any())
            {
                _logger.LogWarning("No roles found for user {UserId}", userId);
                return false;
            }

            var rolesValid = roles.All(requiredRole => 
                userRoles.Any(userRole => userRole.Name.Equals(requiredRole, StringComparison.Ordinal)));

            _logger.LogInformation(
                "Role validation status for user {UserId}: {Status}",
                userId,
                rolesValid ? "Success" : "Failure");

            return rolesValid;
        }
        catch (Exception ex)
        {
            _logger.LogSecurityEvent(SecurityEventIds.RoleValidationFailure, ex, ex.Message, token,
                new Dictionary<string, string[]>
                {
                    ["roles"] = roles
                });
            return false;
        }
    }

    public async Task InvalidateUserRolesCacheAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }

        var cacheKey = GetCacheKey(userId);
        _cache.Remove(cacheKey);
        _logger.LogInformation("Cache invalidated for user {UserId}", userId);
    }

    private async Task<IEnumerable<Role>> GetUserRolesFromCacheAsync(string userId, CancellationToken ct)
    {
        var cacheKey = GetCacheKey(userId);

        if (_cache.TryGetValue(cacheKey, out IEnumerable<Role>? cachedRoles) && cachedRoles != null)
        {
            _logger.LogDebug("Cache hit for user roles {UserId}", userId);
            return cachedRoles;
        }

        _logger.LogDebug("Cache miss for user roles {UserId}, fetching from Keycloak", userId);
        var roles = (await _keycloakService.GetRolesForUserAsync(userId, ct)).ToList();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_cacheDuration)
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        _cache.Set(cacheKey, roles, cacheEntryOptions);
        return roles;
    }

    private static string GetCacheKey(string userId) => $"{CacheKeyPrefix}{userId}";
}