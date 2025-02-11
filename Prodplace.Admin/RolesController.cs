using Microsoft.AspNetCore.Mvc;
using RoleAdmin;

namespace Prodplace.Admin;

[ApiController]
public class RolesController : ControllerBase
{
    [HttpPost("/roles")]
    public async Task<ActionResult<RoleResponse>> CreateRole([FromBody] string roleName,
        [FromServices] RoleAdminService.RoleAdminServiceClient roleAdminServiceClient, CancellationToken ct)
    {
        var newRole = await roleAdminServiceClient.CreateRoleAsync(new CreateRoleRequest
        {
            Name = roleName
        }, cancellationToken: ct);
        return Ok(new CreateRoleDto(newRole.Id, newRole.Name));
    }

    [HttpPost("/roles/assign")]
    public async Task<ActionResult<AssignRoleResponse>> AssignRole([FromBody] AssignRoleRequestDto dto,
        [FromServices] RoleAdminService.RoleAdminServiceClient roleAdminServiceClient, CancellationToken ct)
    {
        var assignResult = await roleAdminServiceClient.AssignRoleAsync(new AssignRoleRequest
        {
            RoleName = dto.RoleName,
            UserId = dto.UserId,
        }, cancellationToken: ct);
        return Ok(new AssignRoleDto(assignResult.Success));
    }
}

public record AssignRoleRequestDto(string UserId, string RoleName);

public record CreateRoleDto(string Id, string Name);

public record AssignRoleDto(bool Success);