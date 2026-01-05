using Microsoft.AspNetCore.Mvc;
using RoleAdmin;

namespace Prodplace.Admin;

[ApiController]
[Route("backoffice")]
public class RolesController : ControllerBase
{
    [HttpGet("/roles")]
    public async Task<ActionResult<IEnumerable<RoleInfo>>> GetRoles(
        [FromServices] RoleAdminService.RoleAdminServiceClient roleAdminServiceClient,
        CancellationToken ct)
    {
        var response = await roleAdminServiceClient.ListRolesAsync(new Empty(), cancellationToken: ct);
        return Ok(response.Roles);
    }

    [HttpGet("/roles/users")]
    public async Task<ActionResult<IEnumerable<UserWithRolesInfo>>> GetUsersWithRoles(
        [FromServices] RoleAdminService.RoleAdminServiceClient roleAdminServiceClient,
        CancellationToken ct)
    {
        var response = await roleAdminServiceClient.ListUsersWithRolesAsync(new Empty(), cancellationToken: ct);
        return Ok(response.Users);
    }

    [HttpPost("/roles")]
    public async Task<ActionResult<RoleResponse>> CreateRole([FromBody] CreateRoleRequestDto role,
        [FromServices] RoleAdminService.RoleAdminServiceClient roleAdminServiceClient, CancellationToken ct)
    {
        var newRole = await roleAdminServiceClient.CreateRoleAsync(new CreateRoleRequest
        {
            Name = role.Name,
            Description = role.Description
        }, cancellationToken: ct);
        return Ok(new CreateRoleDto(newRole.Success));
    }

    [HttpPost("/roles/assign")]
    public async Task<ActionResult<AssignRoleResponse>> AssignRole([FromBody] AssignRoleRequestDto dto,
        [FromServices] RoleAdminService.RoleAdminServiceClient roleAdminServiceClient, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId))
        {
            return BadRequest("UserId cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(dto.RoleName))
        {
            return BadRequest("RoleName cannot be null or empty");
        }

        var assignResult = await roleAdminServiceClient.AssignRoleAsync(new AssignRoleRequest
        {
            UserId = dto.UserId,
            RoleName = dto.RoleName
        }, cancellationToken: ct);

        return Ok(new AssignRoleDto(assignResult.Success));
    }
}

public record AssignRoleRequestDto(string UserId, string RoleName);

public record CreateRoleRequestDto(string Name, string Description);

public record CreateRoleDto(bool Success);

public record AssignRoleDto(bool Success);
