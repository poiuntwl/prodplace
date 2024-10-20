namespace MessagingTools.Contracts;

public record UserRegisteredEvent
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}