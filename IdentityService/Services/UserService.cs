using AuthTools.Constants;
using CommonModels.OutboxModels;
using IdentityService.Dtos;
using IdentityService.Exceptions;
using Keycloak.Net.Core.Models.Root;
using MassTransit;
using MessagingTools.Contracts;

namespace IdentityService.Services;

public interface IUserService
{
    Task<UserDataResult> SignInAsync(RegisterDto registerDto, CancellationToken ct);
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

    public async Task<UserDataResult> SignInAsync(RegisterDto registerDto, CancellationToken ct)
    {
        var userId = await _keycloakService.RegisterAsync(registerDto, ct);
        if (userId == null)
        {
            throw new RegisterUserException();
        }

        var existingRole = await _keycloakService.GetRoleByNameAsync(RoleNames.User, ct, forClient: false);
        if (existingRole == null)
        {
            throw new RegisterUserException(["No user role exists."]);
        }

        await _keycloakService.AssignRoleAsync(userId, RoleNames.User, ct, forClient: false);

        var userDataResult = new UserDataResult
        {
            UserId = userId,
            Email = registerDto.Email,
            Token = _tokenService.CreateToken(new CreateTokenDto
            {
                Email = registerDto.Email,
                Password = registerDto.Password,
                UserId = userId
            })
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

        return userDataResult;
    }

    public async Task<Token> LoginUserAsync(LoginDto loginDto, CancellationToken ct)
    {
        return await _keycloakService.AuthenticateAsync(loginDto.Email, loginDto.Password, ct);
    }
}