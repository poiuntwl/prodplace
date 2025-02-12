using System.Transactions;
using AuthTools.Constants;
using CommonModels.OutboxModels;
using IdentityService.Dtos;
using IdentityService.Exceptions;
using IdentityService.Extensions;
using Keycloak.Net.Core.Models.Root;
using MassTransit;
using MessagingTools.Contracts;

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
    private readonly IKeycloakService _keycloakService;
    private readonly IPublishEndpoint _publishEndpoint;

    public UserService(ITokenService tokenService, IOutboxService outboxService, IKeycloakService keycloakService,
        IPublishEndpoint publishEndpoint)
    {
        _tokenService = tokenService;
        _outboxService = outboxService;
        _keycloakService = keycloakService;
        _publishEndpoint = publishEndpoint;
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

            await _keycloakService.AssignRoleAsync(userId, UserRole.User.GetDescription(), ct);

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
            var message = await _outboxService.CreateOutboxMessageAsync("identity.registerUser", eventData, ct);
            await _publishEndpoint.Publish(new OutboxMessagePostedEvent
            {
                OutboxMessage = message
            }, ct);

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