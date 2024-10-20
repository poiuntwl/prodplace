namespace MessagingTools.Contracts;

public record CustomerCreatedEvent
{
    public required int ConsumerId { get; set; }
    public required DateTime CreatedOnUtc { get; set; }
}