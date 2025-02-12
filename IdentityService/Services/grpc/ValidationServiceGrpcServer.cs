using Grpc.Core;
using IdentityGrpc.Server;

namespace IdentityService.Services.grpc;

public class ValidationServiceGrpcServer : IdentityGrpc.Server.IdentityService.IdentityServiceBase
{
    private readonly IValidationService _validationService;

    public ValidationServiceGrpcServer(IValidationService validationService)
    {
        _validationService = validationService;
    }

    public override async Task<ValidateResponse> ValidateRoles(ValidateRolesRequest request, ServerCallContext context)
    {
        var isValid = await _validationService.ValidateRolesAsync(request.Token, request.Roles.ToArray(), context.CancellationToken);

        return new ValidateResponse
        {
            IsValid = isValid
        };
    }
}