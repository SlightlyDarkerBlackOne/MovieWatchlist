using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MovieWatchlist.Application.Features.Auth.Commands.CreateRefreshToken;
using MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;
using MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;
using MovieWatchlist.Application.Features.Auth.Commands.Login;
using MovieWatchlist.Application.Features.Auth.Commands.Logout;
using MovieWatchlist.Application.Features.Auth.Commands.RefreshToken;
using MovieWatchlist.Application.Features.Auth.Commands.Register;
using MovieWatchlist.Application.Features.Auth.Commands.ValidateToken;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Application.Features.Auth.Queries.GetCurrentUser;

namespace MovieWatchlist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(result.Value);
    }

    [HttpPost("create-refresh-token")]
    public async Task<ActionResult<RefreshTokenResult>> CreateRefreshToken([FromBody] CreateRefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken()
    {
        var command = new RefreshTokenCommand();
        var result = await _mediator.Send(command);

        return Ok(result.Value);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<LogoutResponse>> Logout()
    {
        var command = new LogoutCommand();
        var result = await _mediator.Send(command);

        return Ok(result.Value);
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ValidateTokenResponse>> ValidateToken()
    {
        var command = new ValidateTokenCommand();
        var result = await _mediator.Send(command);

        return Ok(result.Value);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<PasswordResetResponse>> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(result.Value);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<PasswordResetResponse>> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(result.Value);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> Me()
    {
        var query = new GetCurrentUserQuery();
        var result = await _mediator.Send(query);

        return Ok(result.Value);
    }
}

