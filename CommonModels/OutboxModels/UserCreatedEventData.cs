namespace CommonModels.OutboxModels;

public class UserCreatedEventData
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }
}