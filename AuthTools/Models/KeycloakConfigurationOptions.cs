namespace AuthTools.Models;

public class KeycloakConfigurationOptions
{
    public required string ServerUrl { get; set; }
    public required string Realm { get; set; }
    public required string ClientId { get; set; }
    public required string Secret { get; set; }
    public required string AdminUsername { get; set; }
    public required string AdminPassword { get; set; }
}