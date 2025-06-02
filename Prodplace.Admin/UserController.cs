using Microsoft.AspNetCore.Mvc;
using UserAdmin;

namespace Prodplace.Admin;

[ApiController]
public class UserController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser(
        [FromBody] UserDto userDto,
        [FromServices] UserAdminService.UserAdminServiceClient userAdminServiceClient)
    {
        var userResponse = await userAdminServiceClient.CreateUserAsync(new CreateUserRequest
        {
            Email = userDto.Email,
            Name = userDto.Name,
        });
        if (userResponse.Status != UserAdmin.StatusCode.Ok)
        {
            var actionResult = userResponse.Status switch
            {
                UserAdmin.StatusCode.NotFound => NotFound() as ActionResult,
                UserAdmin.StatusCode.AlreadyExists => BadRequest(),
                UserAdmin.StatusCode.InvalidRequest => BadRequest(),
                _ => BadRequest(userResponse.ErrorMessage)
            };

            return actionResult;
        }

        var response = new UserResponse
        {
            UserId = userResponse.User.UserId,
            Name = userResponse.User.Name,
            Email = userResponse.User.Email,
            Roles = userResponse.User.Roles,
            Groups = userResponse.User.Groups
        };
        return Ok(response);
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult<DeleteUserResponse>> DeleteUser(
        string userId,
        [FromServices] UserAdminService.UserAdminServiceClient userAdminServiceClient)
    {
        var deleteResponse = await userAdminServiceClient.DeleteUserAsync(new DeleteUserRequest
        {
            Name = userId
        });
        if (deleteResponse.Status != UserAdmin.StatusCode.Ok)
        {
            var actionResult = deleteResponse.Status switch
            {
                UserAdmin.StatusCode.NotFound => NotFound() as ActionResult,
                UserAdmin.StatusCode.InvalidRequest => BadRequest(),
                _ => BadRequest(deleteResponse.ErrorMessage)
            };

            return actionResult;
        }

        var response = new DeleteUserResponse
        {
            Status = deleteResponse.Status,
            ErrorMessage = deleteResponse.ErrorMessage
        };
        return Ok(response);
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserResponse>> GetUser(
        string userId,
        [FromServices] UserAdminService.UserAdminServiceClient userAdminServiceClient)
    {
        var userResponse = await userAdminServiceClient.GetUserAsync(new GetUserRequest
        {
            UserId = userId
        });
        if (userResponse.Status != UserAdmin.StatusCode.Ok)
        {
            var actionResult = userResponse.Status switch
            {
                UserAdmin.StatusCode.NotFound => NotFound() as ActionResult,
                UserAdmin.StatusCode.InvalidRequest => BadRequest(),
                _ => BadRequest(userResponse.ErrorMessage)
            };

            return actionResult;
        }

        var response = new UserResponse
        {
            UserId = userResponse.User.UserId,
            Name = userResponse.User.Name,
            Email = userResponse.User.Email,
            Roles = userResponse.User.Roles,
            Groups = userResponse.User.Groups
        };
        return Ok(response);
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(
        string userId,
        [FromBody] UpdateUserDto updateUserDto,
        [FromServices] UserAdminService.UserAdminServiceClient userAdminServiceClient)
    {
        var userResponse = await userAdminServiceClient.UpdateUserAsync(new UpdateUserRequest
        {
            UserId = userId,
            Name = updateUserDto.Name,
            Description = updateUserDto.Description
        });
        if (userResponse.Status != UserAdmin.StatusCode.Ok)
        {
            var actionResult = userResponse.Status switch
            {
                UserAdmin.StatusCode.NotFound => NotFound() as ActionResult,
                UserAdmin.StatusCode.InvalidRequest => BadRequest(),
                _ => BadRequest(userResponse.ErrorMessage)
            };

            return actionResult;
        }

        var response = new UserResponse
        {
            UserId = userResponse.User.UserId,
            Name = userResponse.User.Name,
            Email = userResponse.User.Email,
            Roles = userResponse.User.Roles,
            Groups = userResponse.User.Groups
        };
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<ListUsersResponse>> ListUsers(
        [FromServices] UserAdminService.UserAdminServiceClient userAdminServiceClient,
        [FromQuery] int pageSize = 10,
        [FromQuery] string pageToken = "")
    {
        var listResponse = await userAdminServiceClient.ListUsersAsync(new ListUsersRequest
        {
            PageSize = pageSize,
            PageToken = pageToken
        });
        if (listResponse.Users == null)
        {
            return BadRequest("Invalid response from service");
        }

        var users = listResponse.Users.Select(u => new UserResponse
        {
            UserId = u.UserId,
            Name = u.Name,
            Email = u.Email,
            Roles = u.Roles,
            Groups = u.Groups
        }).ToList();

        var response = new ListUsersResponse
        {
            Users = users,
            NextPageToken = listResponse.NextPageToken
        };
        return Ok(response);
    }
}

public record UserDto(string Email, string Name);

public record UpdateUserDto(string Name, string Description);

public record UserResponse
{
    public required string UserId { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public IEnumerable<string>? Roles { get; init; }
    public IEnumerable<string>? Groups { get; init; }
};

public record DeleteUserResponse
{
    public StatusCode Status { get; init; }
    public string? ErrorMessage { get; init; }
};

public record ListUsersResponse
{
    public IEnumerable<UserResponse> Users { get; init; } = [];
    public string NextPageToken { get; init; } = "";
};