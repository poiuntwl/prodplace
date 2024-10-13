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
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<AppUser> _signInManager;

    public LoginUserHandler(UserManager<AppUser> userManager, ITokenService tokenService,
        SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _signInManager = signInManager;
    }

    public async Task<UserDataResult> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var loginDto = request.LoginDto;

        var user = _userManager.Users.FirstOrDefault(x => x.UserName == loginDto.Username || x.Email == loginDto.Email);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (result.Succeeded == false)
        {
            throw new UnauthorizedAccessException();
        }

        return new UserDataResult
        {
            Email = user.Email,
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        };
    }
}