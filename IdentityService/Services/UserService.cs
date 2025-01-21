using System.Transactions;
using AuthTools.Constants;
using CommonModels.OutboxModels;
using IdentityService.Dtos;
using IdentityService.Exceptions;
using IdentityService.Models;
using Keycloak.Net.Core.Models.Root;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services;

public interface IUserService
{
    Task<UserDataResult> RegisterUserAsync(RegisterDto registerDto, CancellationToken ct);
    Task<Token> LoginUserAsync(LoginDto loginDto, CancellationToken ct);
}

public class UserService : IUserService
{
    private readonly ITokenService _tokenService;
    private readonly IOutboxService _outboxService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IKeycloakService _keycloakService;

    public UserService(ITokenService tokenService, IOutboxService outboxService,
        SignInManager<AppUser> signInManager, IKeycloakService keycloakService)
    {
        _tokenService = tokenService;
        _outboxService = outboxService;
        _signInManager = signInManager;
        _keycloakService = keycloakService;
    }

    public async Task<UserDataResult> RegisterUserAsync(RegisterDto registerDto, CancellationToken ct)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var userId = await _keycloakService.RegisterAsync(registerDto, ct);
            if (userId == null)
            {
                throw new RegisterUserException();
            }

            await _keycloakService.AssignRoleAsync(userId, UserRole.User, ct);

            var userDataResult = new UserDataResult
            {
                Email = registerDto.Email,
                Token = _tokenService.CreateToken(registerDto)
            };

            var eventData = new UserCreatedEventData
            {
                Email = userDataResult.Email,
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

    public async Task<Token> LoginUserAsync(LoginDto loginDto, CancellationToken ct)
    {
        return await _keycloakService.AuthenticateAsync(loginDto.Email, loginDto.Password, ct);
        /*
        var user = await _keycloakService.AuthenticateAsync(loginDto.Email, loginDto.Password, ct);
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
    */
    }
}