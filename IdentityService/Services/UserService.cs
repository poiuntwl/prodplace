using System.Transactions;
using CommonModels.OutboxModels;
using IdentityService.Constants;
using IdentityService.Dtos;
using IdentityService.Exceptions;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services;

public interface IUserService
{
    Task<UserDataResult> RegisterUserAsync(RegisterDto registerDto, CancellationToken ct);
    Task<UserDataResult> LoginUserAsync(LoginDto loginDto, CancellationToken ct);
}

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IOutboxService _outboxService;
    private readonly SignInManager<AppUser> _signInManager;

    public UserService(UserManager<AppUser> userManager, ITokenService tokenService, IOutboxService outboxService,
        SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _outboxService = outboxService;
        _signInManager = signInManager;
    }

    public async Task<UserDataResult> RegisterUserAsync(RegisterDto registerDto, CancellationToken ct)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

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

            var userDataResult = new UserDataResult
            {
                Username = appUser.UserName,
                Email = appUser.Email,
                Token = _tokenService.CreateToken(appUser)
            };

            var eventData = new UserCreatedEventData
            {
                Email = userDataResult.Email,
                Username = userDataResult.Username,
                FirstName = userDataResult.Username,
                LastName = null
            };
            await _outboxService.CreateOutboxMessageAsync("identity.registerUser", eventData, ct);

            scope.Complete();

            return userDataResult;
        }
        catch (Exception e)
        {
            throw new RegisterUserException(new List<string>
            {
                e.Message
            });
        }
    }

    public async Task<UserDataResult> LoginUserAsync(LoginDto loginDto, CancellationToken ct)
    {
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