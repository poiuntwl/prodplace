using IdentityService.Requests;
using MediatR.Pipeline;

namespace IdentityService.Handlers.Preprocessors;

public class RegisterUserMessagingPreprocessor : IRequestPreProcessor<RegisterUserRequest>
{
    public async Task Process(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}