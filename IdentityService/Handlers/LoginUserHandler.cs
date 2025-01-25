using IdentityService.Dtos;
using IdentityService.Services;
using Keycloak.Net.Core.Models.Root;
using MediatR;

namespace IdentityService.Handlers;

public class LoginUserHandler : IRequestHandler<LoginUserRequest, Token>
{
    private readonly IUserService _userService;

    public LoginUserHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Token> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        return await _userService.LoginUserAsync(request.LoginDto, cancellationToken);
    }
}

public record LoginUserRequest(LoginDto LoginDto) : IRequest<Token>;