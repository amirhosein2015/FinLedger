using Asp.Versioning;
using FinLedger.Modules.Identity.Application.Users.Login;
using FinLedger.Modules.Identity.Application.Users.RegisterUser;
using FinLedger.Modules.Identity.Application.Users.AssignTenantRole; // Added missing namespace
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinLedger.Modules.Identity.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/identity/[controller]")]
[ApiController]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new user globally in the FinLedger platform.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        var userId = await mediator.Send(command);
        return Ok(new { UserId = userId });
    }

    /// <summary>
    /// Authenticates a user and returns a multi-tenant JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var token = await mediator.Send(command);
        return Ok(new { AccessToken = token });
    }

    /// <summary>
    /// Assigns a specific role to a user for a specific tenant.
    /// Required before the user can access tenant-specific ledger data.
    /// </summary>
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(AssignTenantRoleCommand command)
    {
        await mediator.Send(command);
        return Ok(new { Message = "Role assigned successfully." });
    }
}

