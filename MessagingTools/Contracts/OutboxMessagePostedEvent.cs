using IdentityService.Models;

namespace MessagingTools.Contracts;

public record OutboxMessagePostedEvent
{
    public required OutboxMessage OutboxMessage { get; set; }
}