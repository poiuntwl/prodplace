using Grpc.Core;
using RoleAdmin;

namespace IdentityService.Services.grpc;

public class RoleAdminGrpcServer : RoleAdminService.RoleAdminServiceBase
{
    public override async Task<RoleResponse> CreateRole(CreateRoleRequest request, ServerCallContext context)
    {
        return new RoleResponse();
    }

    public override async Task<DeleteRoleResponse> DeleteRole(DeleteRoleRequest request, ServerCallContext context)
    {
        return new DeleteRoleResponse();
    }

    public override async Task<AssignRoleResponse> AssignRole(AssignRoleRequest request, ServerCallContext context)
    {
        return new AssignRoleResponse();
    }

    public override async Task<UnassignRoleResponse> UnassignRole(UnassignRoleRequest request, ServerCallContext context)
    {
        return new UnassignRoleResponse();
    }
}