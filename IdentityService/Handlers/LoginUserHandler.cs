using IdentityService.Dtos;
using IdentityService.Exceptions;
using IdentityService.Models;
using IdentityService.Requests;
using IdentityService.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

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