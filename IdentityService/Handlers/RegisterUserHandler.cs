using IdentityService.Constants;
using IdentityService.Dtos;
using IdentityService.Exceptions;
using IdentityService.Models;
using IdentityService.Requests;
using IdentityService.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Handlers;

public class RegisterUserHandler : IRequestHandler<RegisterUserRequest, UserDataResult>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public RegisterUserHandler(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<UserDataResult> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var registerDto = request.RegisterDto;

        try
        {
            var appUser = new AppUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Username
            };

            var createResult = await _userManager.CreateAsync(appUser, registerDto.Password);

            if (createResult.Succeeded == false)
            {
                throw new RegisterUserException(createResult.Errors.Select(x => x.Description).ToList());
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(appUser, AppRoles.User);
            if (addToRoleResult.Succeeded == false)
            {
                throw new RegisterUserException(createResult.Errors.Select(x => x.Description).ToList());
            }

            return new UserDataResult
            {
                Username = appUser.UserName,
                Email = appUser.Email,
                Token = _tokenService.CreateToken(appUser)
            };
        }
        catch (Exception e)
        {
            throw new RegisterUserException(new List<string>
            {
                e.Message
            });
        }
    }
}