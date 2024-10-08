using IdentityService.Constants;
using IdentityService.Dtos;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[Route("api/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<AppUser> _userManager;

    public AccountController(ITokenService tokenService, UserManager<AppUser> userManager)
    {
        _tokenService = tokenService;
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
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

            var createResult = await _userManager.CreateAsync(appUser, registerDto.Password);

            if (createResult.Succeeded == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    errors = createResult.Errors.Select(x => x.Description)
                });
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(appUser, AppRoles.User);
            if (addToRoleResult.Succeeded == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    errors = addToRoleResult.Errors.Select(x => x.Description)
                });
            }

            return Ok(new NewUserResult
            {
                Username = appUser.UserName,
                Email = appUser.Email,
                Token = _tokenService.CreateToken(appUser)
            });
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = e.Message
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginDto loginDto,
        [FromServices] SignInManager<AppUser> signInManager)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        var user = _userManager.Users.FirstOrDefault(x => x.UserName == loginDto.Username || x.Email == loginDto.Email);
        if (user == null)
        {
            return NotFound();
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (result.Succeeded == false)
        {
            return Unauthorized();
        }

        return Ok(new NewUserResult
        {
            Email = user.Email,
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        });
    }
}