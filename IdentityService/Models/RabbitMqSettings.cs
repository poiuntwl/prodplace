namespace IdentityService.Models;

public class RabbitMqSettings
{
    public string QueueName { get; set; }
    public string HostName { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}