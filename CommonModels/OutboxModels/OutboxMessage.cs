namespace IdentityService.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}