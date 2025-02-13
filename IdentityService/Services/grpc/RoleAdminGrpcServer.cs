using Grpc.Core;
using RoleAdmin;

namespace IdentityService.Services.grpc;

public class RoleAdminGrpcServer : RoleAdminService.RoleAdminServiceBase
{
    private readonly IRoleService _roleService;

    public RoleAdminGrpcServer(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public override async Task<RoleResponse> CreateRole(CreateRoleRequest request, ServerCallContext context)
    {
        var roleCreated =
            await _roleService.CreateRoleAsync(request.Name, request.Description, context.CancellationToken);
        return new RoleResponse
        {
            Success = roleCreated
        };
    }

    public override async Task<DeleteRoleResponse> DeleteRole(DeleteRoleRequest request, ServerCallContext context)
    {
        var success = await _roleService.DeleteRoleAsync(request.Name, context.CancellationToken);
        return new DeleteRoleResponse
        {
            Success = success
        };
    }

    public override async Task<AssignRoleResponse> AssignRole(AssignRoleRequest request, ServerCallContext context)
    {
        var assigned = await _roleService.AssignRoleAsync(request.UserId, request.RoleName, context.CancellationToken);
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
        return new UnassignRoleResponse
        {
            Success = unassigned
        };
    }
}