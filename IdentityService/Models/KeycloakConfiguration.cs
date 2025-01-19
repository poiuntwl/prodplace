namespace IdentityService.Models;

public class KeycloakConfiguration
{
    public string ServerUrl { get; set; }
    public string Realm { get; set; }
    public string AdminUsername { get; set; }
    public string AdminPassword { get; set; }
}