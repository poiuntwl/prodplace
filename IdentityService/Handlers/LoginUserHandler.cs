using IdentityService.Dtos;
using IdentityService.Services;
using MediatR;

namespace IdentityService.Handlers;

public class LoginUserHandler : IRequestHandler<LoginUserRequest, UserDataResult>
{
    private readonly IUserService _userService;

    public LoginUserHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserDataResult> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        return await _userService.LoginUserAsync(request.LoginDto, cancellationToken);
    }
}

public record LoginUserRequest(LoginDto LoginDto) : IRequest<UserDataResult>;