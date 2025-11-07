using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mapster;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Api.Helpers;
using MovieWatchlist.Api.Constants;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Constants;
using ApiUserInfo = MovieWatchlist.Api.DTOs.UserInfo;
using ApiPasswordResetResponse = MovieWatchlist.Api.DTOs.PasswordResetResponse;

namespace MovieWatchlist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthCookieManager _authCookieManager;

    public AuthController(
        IMediator mediator,
        ILogger<AuthController> logger,
        IAuthCookieManager authCookieManager)
    {
        _mediator = mediator;
        _logger = logger;
        _authCookieManager = authCookieManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterDto dto)
    {
        var command = dto.Adapt<RegisterCommand>();
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        _logger.LogInformation("User registered successfully: {Username}", dto.Username);

        if (result.Value == null)
            return BadRequest(new { error = "Invalid authentication result" });

        return BuildAuthResponse<RegisterResponse>(result.Value);
    }

    [HttpPost("create-refresh-token")]
    public async Task<ActionResult<RefreshTokenResult>> CreateRefreshToken([FromBody] CreateRefreshTokenDto dto)
    {
        var command = new CreateRefreshTokenCommand(dto.UserId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = dto.Adapt<LoginCommand>();
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        if (result.Value == null)
            return BadRequest(new { error = "Invalid authentication result" });

        return BuildAuthResponse<LoginResponse>(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken()
    {
        var refreshToken = GetTokenFromCookie(CookieNames.RefreshToken);
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token not provided" });

        var command = new RefreshTokenCommand(refreshToken);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return Unauthorized(new { message = result.Error });

        if (result.Value == null)
            return BadRequest(new { error = "Invalid authentication result" });

        return BuildAuthResponse<RefreshTokenResponse>(result.Value);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<LogoutResponse>> Logout()
    {
        var token = GetTokenFromCookie(CookieNames.AccessToken);
        if (!string.IsNullOrEmpty(token))
        {
            var command = new LogoutCommand(token);
            var result = await _mediator.Send(command);
            if (result.IsFailure)
                return BadRequest(new { message = result.Error });
            if (!result.Value)
                return BadRequest(new { message = ErrorMessages.LogoutFailed });
        }

        _authCookieManager.ClearAuthCookies(Response);
        return Ok(new LogoutResponse(ErrorMessages.LogoutSuccess));
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ValidateTokenResponse>> ValidateToken()
    {
        var token = GetTokenFromHeader();
        if (string.IsNullOrEmpty(token))
            return BadRequest(new ValidateTokenResponse(false));

        var command = new ValidateTokenCommand(token);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new ValidateTokenResponse(false));

        return Ok(new ValidateTokenResponse(result.Value));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiPasswordResetResponse>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Password reset requested for email: {Email}", dto.Email);

        var command = dto.Adapt<ForgotPasswordCommand>();
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var response = result.Value.Adapt<ApiPasswordResetResponse>();
        return Ok(response);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiPasswordResetResponse>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest(new { Message = ErrorMessages.ResetTokenRequired });

        _logger.LogInformation("Password reset attempt with token: {Token}", dto.Token.Substring(0, Math.Min(8, dto.Token.Length)) + "...");

        var command = dto.Adapt<ResetPasswordCommand>();
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var response = result.Value.Adapt<ApiPasswordResetResponse>();
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiUserInfo>> Me()
    {
        var query = new GetCurrentUserQuery();
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            if (result.Error == "User not authenticated")
                return Unauthorized();
            if (result.Error == "User not found")
                return NotFound();
            return BadRequest(new { error = result.Error });
        }

        if (result.Value == null)
            return BadRequest(new { error = "User information not available" });

        var userInfo = result.Value.Adapt<ApiUserInfo>();
        return Ok(userInfo);
    }

    private ActionResult<TResponse> BuildAuthResponse<TResponse>(AuthenticationResult auth) where TResponse : class
    {
        if (auth?.User == null || !auth.ExpiresAt.HasValue)
            return BadRequest(new { error = "Invalid authentication result" });

        _authCookieManager.SetAuthCookies(Response, auth);
        var response = auth.Adapt<TResponse>();

        return Ok(response);
    }
}

