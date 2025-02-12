namespace AuthTools.Models;

public class KeycloakOptions
{
    public string ServerUrl { get; set; }
    public string Realm { get; set; }
    public string ClientId { get; set; }
    public string Secret { get; set; }
    public string AdminUsername { get; set; }
    public string AdminPassword { get; set; }
}