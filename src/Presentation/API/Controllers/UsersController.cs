using Application.Features.Users;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id));
        if (user is null)
            return NotFound();
        return Ok(user);
    }

    [HttpPost("register")]
    public async Task<ActionResult<int>> Register(RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginUserCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return Unauthorized(result.Error);
        return Ok(result.Value);
    }
}
