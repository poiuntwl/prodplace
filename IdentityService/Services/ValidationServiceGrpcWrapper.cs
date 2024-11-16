using AuthTools;
using Grpc.Core;
using IdentityGrpc.Server;

namespace IdentityService.Services;

public class ValidationServiceGrpcWrapper : IdentityGrpc.Server.IdentityService.IdentityServiceBase
{
    private readonly IValidationService _validationService;

    public ValidationServiceGrpcWrapper(IValidationService validationService, IJwtValidator jwtValidator)
    {
        _validationService = validationService;
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