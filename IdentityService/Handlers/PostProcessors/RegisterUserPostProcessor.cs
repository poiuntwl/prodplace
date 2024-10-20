using IdentityService.Dtos;
using IdentityService.Requests;
using MassTransit;
using MediatR.Pipeline;
using MessagingTools.Contracts;

namespace IdentityService.Handlers.PostProcessors;

public class RegisterUserPostProcessor : IRequestPostProcessor<RegisterUserRequest, UserDataResult>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterUserPostProcessor(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Process(RegisterUserRequest request, UserDataResult response, CancellationToken cancellationToken)
    {
        await _publishEndpoint.Publish(new UserRegisteredEvent
        {
            Email = response.Email,
            Username = response.Username,
            CreatedOnUtc = DateTime.UtcNow
        }, cancellationToken);
    }
}