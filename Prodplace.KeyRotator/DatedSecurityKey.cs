using Microsoft.IdentityModel.Tokens;

namespace Prodplace.KeyRotator;

public class DatedSecurityKey
{
    public required SecurityKey Key { get; init; }
    public required DateTimeOffset Created { get; init; }
    public DateTimeOffset Expires { get; init; }
}