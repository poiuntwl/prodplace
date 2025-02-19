using AuthTools.Constants;
using Grpc.Core;
using RoleAdmin;

namespace IdentityService.Services.grpc;

public class RoleAdminGrpcServer : RoleAdminService.RoleAdminServiceBase
{
    private readonly IRoleService _roleService;
    private readonly IValidationService _validationService;

    public RoleAdminGrpcServer(IRoleService roleService, IValidationService validationService)
    {
        _roleService = roleService;
        _validationService = validationService;
    }

    public override async Task<RoleResponse> CreateRole(CreateRoleRequest request, ServerCallContext context)
    {
        var reqUser = context.GetHttpContext().User;
        if (reqUser.IsInRole(RoleNames.Admin))
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Insufficient permissions"));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Role cannot be empty"));
        }

        if (request.Name.Length > 64)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                "Role name exceeds maximum length of 64 characters"));
        }

        var roleCreated =
            await _roleService.CreateRoleAsync(request.Name, request.Description, context.CancellationToken);
        return new RoleResponse
        {
            Success = roleCreated
        };
    }

    public override async Task<DeleteRoleResponse> DeleteRole(DeleteRoleRequest request, ServerCallContext context)
    {
        var usersWithRole = await _roleService.GetUsersWithRoleAsync(request.Name, context.CancellationToken);

        var success = await _roleService.DeleteRoleAsync(request.Name, context.CancellationToken);

        if (!success)
        {
            return new DeleteRoleResponse
            {
                Success = success
            };
        }

        foreach (var userId in usersWithRole)
        {
            await _validationService.InvalidateUserRolesCacheAsync(userId);
        }

        return new DeleteRoleResponse
        {
            Success = success
        };
    }

    public override async Task<AssignRoleResponse> AssignRole(AssignRoleRequest request, ServerCallContext context)
    {
        var assigned = await _roleService.AssignRoleAsync(request.UserId, request.RoleName, context.CancellationToken);
        if (assigned)
        {
            await _validationService.InvalidateUserRolesCacheAsync(request.UserId);
        }

        return new AssignRoleResponse
        {
            Success = assigned
        };
    }

    public override async Task<UnassignRoleResponse> UnassignRole(UnassignRoleRequest request,
        ServerCallContext context)
    {
        var unassigned =
            await _roleService.UnassignRoleAsync(request.UserId, request.RoleName, context.CancellationToken);
        if (unassigned)
        {
            await _validationService.InvalidateUserRolesCacheAsync(request.UserId);
        }

        return new UnassignRoleResponse
        {
            Success = unassigned
        };
    }
}