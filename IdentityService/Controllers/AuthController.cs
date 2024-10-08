using System.Text.Json;
using IdentityService.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IValidationService _validationService;

    public AuthController(IValidationService validationService)
    {
        _validationService = validationService;
    }

    [HttpGet("validate-token")]
    public IActionResult ValidateToken([FromQuery] string token, CancellationToken ct)
    {
        try
        {
            var isValid = _validationService.ValidateToken(token);
            return Ok(new TokenValidationResponse(isValid));
        }
        catch (Exception)
        {
            return Ok(new TokenValidationResponse(false));
        }
    }

    [HttpGet("validate-roles")]
    public async Task<IActionResult> ValidateRoles([FromQuery] string token,
        [FromQuery(Name = "roles")] string rolesString)
    {
        if (string.IsNullOrWhiteSpace(rolesString))
        {
            return Ok(new TokenValidationResponse(false));
        }

        try
        {
            var roles = rolesString.Split(",");
            var isValid = await _validationService.ValidateRolesAsync(token, roles);
            return Ok(new TokenValidationResponse(isValid));
        }
        catch (Exception)
        {
            return Ok(new TokenValidationResponse(false));
        }
    }
}

public record TokenValidationResponse(bool IsValid);

public record RolesValidationResponse(bool IsValid);