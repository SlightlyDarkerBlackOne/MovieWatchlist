using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Exceptions;

namespace MovieWatchlist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResult>> Register([FromBody] RegisterDto dto)
    {
        var command = new RegisterCommand(dto.Username, dto.Email, dto.Password);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        _logger.LogInformation("User registered successfully: {Username}", dto.Username);
        return Ok(result.Value);
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
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginDto dto)
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

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<object>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new RefreshTokenCommand(refreshTokenDto.RefreshToken);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return Unauthorized(new { message = result.Error });

        return Ok(new { token = result.Value });
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var token = GetTokenFromHeader();
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = ErrorMessages.TokenNotProvided });

        var command = new LogoutCommand(token);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        if (!result.Value)
            return BadRequest(new { message = ErrorMessages.LogoutFailed });

        return Ok(new { message = ErrorMessages.LogoutSuccess });
    }

    [HttpPost("validate")]
    public async Task<ActionResult<object>> ValidateToken()
    {
        var token = GetTokenFromHeader();
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = ErrorMessages.TokenNotProvided, isValid = false });

        var command = new ValidateTokenCommand(token);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(new { message = result.Error, isValid = false });

        return Ok(new { isValid = result.Value });
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<PasswordResetResponse>> ForgotPassword([FromBody] ForgotPasswordDto dto)
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

            return Ok(result.Value);
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
    public async Task<ActionResult<PasswordResetResponse>> ResetPassword([FromBody] ResetPasswordDto dto)
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

            return Ok(result.Value);
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
}

