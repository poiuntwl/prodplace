using System.Net;
using IdentityService.Constants;
using IdentityService.Dtos;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[Microsoft.AspNetCore.Components.Route("api/account")]
[ApiController]
public class AccountController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto,
        [FromServices] UserManager<AppUser> userManager,
        [FromServices] ITokenService tokenService)
    {
        try
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            var appUser = new AppUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Username
            };

            var createResult = await userManager.CreateAsync(appUser, registerDto.Password);

            if (createResult.Succeeded == false)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    errors = createResult.Errors.Select(x => x.Description)
                });
            }

            var addToRoleResult = await userManager.AddToRoleAsync(appUser, AppRoles.User);
            if (addToRoleResult.Succeeded)
            {
                return Ok(new RegisterUserResult
                {
                    Username = appUser.UserName,
                    Email = appUser.Email,
                    Token = tokenService.CreateToken(appUser)
                });
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, new
            {
                errors = addToRoleResult.Errors.Select(x => x.Description)
            });
        }
        catch (Exception e)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new
            {
                error = e.Message
            });
        }
    }
}