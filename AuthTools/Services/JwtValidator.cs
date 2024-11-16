namespace AuthTools.Services;

public interface IJwtValidator
{
    bool Validate(string token);
}

public class JwtValidator : IJwtValidator
{
    private readonly IJwtClaimsPrincipalGetter _jwtClaimsPrincipalGetter;

    public JwtValidator(IJwtClaimsPrincipalGetter jwtClaimsPrincipalGetter)
    {
        _jwtClaimsPrincipalGetter = jwtClaimsPrincipalGetter;
    }

    public bool Validate(string token)
    {
        return _jwtClaimsPrincipalGetter.Get(token) != null;
    }
}