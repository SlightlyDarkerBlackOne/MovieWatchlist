using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Exceptions;
using ApiUserInfo = MovieWatchlist.Api.DTOs.UserInfo;
using ApiPasswordResetResponse = MovieWatchlist.Api.DTOs.PasswordResetResponse;

namespace MovieWatchlist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IMediator mediator,
        ILogger<AuthController> logger,
        IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _logger = logger;
        _environment = environment;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterDto dto)
    {
        var command = new RegisterCommand(dto.Username, dto.Email, dto.Password);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        _logger.LogInformation("User registered successfully: {Username}", dto.Username);

        var auth = result.Value;
        if (auth.User == null || !auth.ExpiresAt.HasValue)
            return BadRequest(new { error = "Invalid authentication result" });

        var expiresAt = auth.ExpiresAt.Value;
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment() && !_environment.IsEnvironment("Testing"),
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt
        };

        if (!string.IsNullOrEmpty(auth.Token))
        {
            Response.Cookies.Append("accessToken", auth.Token, cookieOptions);
        }
        if (!string.IsNullOrEmpty(auth.RefreshToken))
        {
            Response.Cookies.Append("refreshToken", auth.RefreshToken, cookieOptions);
        }

        var response = new RegisterResponse(
            new ApiUserInfo(
                auth.User.Id,
                auth.User.Username,
                auth.User.Email,
                auth.User.CreatedAt
            ),
            expiresAt
        );

        return Ok(response);
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

        var command = new LoginCommand(
            UsernameOrEmail: dto.UsernameOrEmail,
            Password: dto.Password
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        var auth = result.Value;
        if (auth.User == null || !auth.ExpiresAt.HasValue)
            return BadRequest(new { error = "Invalid authentication result" });

        var expiresAt = auth.ExpiresAt.Value;
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment() && !_environment.IsEnvironment("Testing"),
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt
        };

        if (!string.IsNullOrEmpty(auth.Token))
        {
            Response.Cookies.Append("accessToken", auth.Token, cookieOptions);
        }
        if (!string.IsNullOrEmpty(auth.RefreshToken))
        {
            Response.Cookies.Append("refreshToken", auth.RefreshToken, cookieOptions);
        }

        var response = new LoginResponse(
            new ApiUserInfo(
                auth.User.Id,
                auth.User.Username,
                auth.User.Email,
                auth.User.CreatedAt
            ),
            expiresAt
        );

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token not provided" });

        var command = new RefreshTokenCommand(refreshToken);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return Unauthorized(new { message = result.Error });

        var auth = result.Value;
        if (auth.User == null || !auth.ExpiresAt.HasValue)
            return BadRequest(new { error = "Invalid authentication result" });

        var expiresAt = auth.ExpiresAt.Value;
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment() && !_environment.IsEnvironment("Testing"),
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt
        };

        if (!string.IsNullOrEmpty(auth.Token))
        {
            Response.Cookies.Append("accessToken", auth.Token, cookieOptions);
        }
        if (!string.IsNullOrEmpty(auth.RefreshToken))
        {
            Response.Cookies.Append("refreshToken", auth.RefreshToken, cookieOptions);
        }

        var response = new RefreshTokenResponse(
            new ApiUserInfo(
                auth.User.Id,
                auth.User.Username,
                auth.User.Email,
                auth.User.CreatedAt
            ),
            expiresAt
        );

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<LogoutResponse>> Logout()
    {
        var token = Request.Cookies["accessToken"];
        if (!string.IsNullOrEmpty(token))
        {
            var command = new LogoutCommand(token);
            var result = await _mediator.Send(command);
            if (result.IsFailure)
                return BadRequest(new { message = result.Error });
            if (!result.Value)
                return BadRequest(new { message = ErrorMessages.LogoutFailed });
        }

        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
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
        try
        {
            if (!ModelState.IsValid)
                throw new ValidationException(ErrorMessages.InvalidModelState, ModelState);

            _logger.LogInformation("Password reset requested for email: {Email}", dto.Email);

            var command = new ForgotPasswordCommand(Email: dto.Email);

            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            var coreResponse = result.Value!;
            var response = new ApiPasswordResetResponse(
                coreResponse.Success,
                coreResponse.Message
            );

            return Ok(response);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            return StatusCode(500, new { message = ErrorMessages.PasswordResetRequestError });
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiPasswordResetResponse>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                throw new ValidationException(ErrorMessages.InvalidModelState, ModelState);

            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest(new { Message = ErrorMessages.ResetTokenRequired });

            _logger.LogInformation("Password reset attempt with token: {Token}", dto.Token.Substring(0, Math.Min(8, dto.Token.Length)) + "...");

            var command = new ResetPasswordCommand(
                Token: dto.Token,
                NewPassword: dto.NewPassword
            );

            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            var coreResponse = result.Value!;
            var response = new ApiPasswordResetResponse(
                coreResponse.Success,
                coreResponse.Message
            );

            return Ok(response);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset request");
            return StatusCode(500, new { message = ErrorMessages.PasswordResetFailed });
        }
    }

    private string? GetTokenFromHeader()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }
        return null;
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

        var userInfo = result.Value!;
        return Ok(new ApiUserInfo(
            userInfo.Id,
            userInfo.Username,
            userInfo.Email,
            userInfo.CreatedAt
        ));
    }
}

