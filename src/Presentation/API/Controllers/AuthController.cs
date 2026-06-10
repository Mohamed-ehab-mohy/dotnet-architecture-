using Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return Unauthorized(result.Error);
        return Ok(new { Token = result.Value });
    }

    [HttpPost("register")]
    public async Task<ActionResult<int>> Register(RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok(new { UserId = result.Value });
    }

    [HttpPost("refresh-token")]
    public ActionResult RefreshToken()
    {
        return Ok(new { Message = "Token refresh endpoint" });
    }
}
