using System.Transactions;
using CommonModels.OutboxModels;
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
    private readonly IOutboxService _outboxService;

    public RegisterUserHandler(UserManager<AppUser> userManager, ITokenService tokenService,
        IOutboxService outboxService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _outboxService = outboxService;
    }

    public async Task<UserDataResult> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

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
            await _outboxService.CreateOutboxMessageAsync("identity.registerUser", eventData, cancellationToken);

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
}