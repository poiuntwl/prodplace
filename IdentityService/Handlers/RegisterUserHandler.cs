using IdentityService.Dtos;
using IdentityService.Services;
using MediatR;

namespace IdentityService.Handlers;

public class RegisterUserHandler : IRequestHandler<RegisterUserRequest, UserDataResult>
{
    private readonly IUserService _userService;

    public RegisterUserHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserDataResult> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        return await _userService.RegisterUserAsync(request.RegisterDto, cancellationToken);
    }
}

public record RegisterUserRequest(RegisterDto RegisterDto) : IRequest<UserDataResult>;