using Grpc.Core;
using IdentityGrpc.Server;

namespace IdentityService.Services;

public class ValidationServiceGrpcWrapper : IdentityGrpc.Server.IdentityService.IdentityServiceBase
{
    private readonly IValidationService _validationService;

    public ValidationServiceGrpcWrapper(IValidationService validationService)
    {
        _validationService = validationService;
    }

    public override Task<ValidateResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        var isValid = _validationService.ValidateToken(request.Token);

        return Task.FromResult(new ValidateResponse
        {
            IsValid = isValid
        });
    }

    public override async Task<ValidateResponse> ValidateRoles(ValidateRolesRequest request, ServerCallContext context)
    {
        var isValid = await _validationService.ValidateRolesAsync(request.Token, request.Roles.ToArray());

        return new ValidateResponse
        {
            IsValid = isValid
        };
    }
}