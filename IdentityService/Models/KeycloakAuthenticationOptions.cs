namespace IdentityService.Models;

public class KeycloakAuthenticationOptions
{
    public string AuthServerUrl { get; set; }
    public string Realm { get; set; }
    public string AdminUsername { get; set; }
    public string AdminPassword { get; set; }
}