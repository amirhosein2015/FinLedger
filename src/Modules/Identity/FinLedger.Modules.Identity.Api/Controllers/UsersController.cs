using Asp.Versioning;
using FinLedger.Modules.Identity.Application.Users.Login;
using FinLedger.Modules.Identity.Application.Users.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Modules.Identity.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/identity/[controller]")]
[ApiController]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        var userId = await mediator.Send(command);
        return Ok(new { UserId = userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var token = await mediator.Send(command);
        return Ok(new { AccessToken = token });
    }
}
