using IdentityService.Dtos;
using IdentityService.Exceptions;
using IdentityService.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[Route("api/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto,
        CancellationToken ct)
    {
        try
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(new RegisterUserRequest(registerDto), ct);

            return Ok(result);
        }
        catch (RegisterUserException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = e.Message
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
        CancellationToken ct)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _mediator.Send(new LoginUserRequest(loginDto), ct);
            return Ok(result);
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}