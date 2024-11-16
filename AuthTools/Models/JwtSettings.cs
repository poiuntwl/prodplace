namespace AuthTools.Models;

public class JwtSettings
{
    public string Audience { get; init; } = string.Empty;
    public string MetadataAddress { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
}