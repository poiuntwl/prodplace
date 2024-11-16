using IdentityService.Dtos;
using IdentityService.Requests;
using IdentityService.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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