using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Application.Validation;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Exceptions;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IInputValidationService _validationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authService, 
        IInputValidationService validationService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _validationService = validationService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResult>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                throw new ValidationException(ErrorMessages.InvalidModelState, ModelState);

            // Sanitize inputs
            var sanitizedUsername = _validationService.SanitizeInput(dto.Username);
            var sanitizedEmail = _validationService.SanitizeInput(dto.Email);

            // Validate inputs
            var validationResult = _validationService.ValidateRegistrationInput(
                sanitizedUsername, sanitizedEmail, dto.Password);

            if (!validationResult.IsValid)
                throw new ValidationException(ErrorMessages.ValidationFailed, validationResult.Errors);

            _logger.LogInformation("Registration attempt for user: {Username}", sanitizedUsername);

            // Map DTO → Command
            var command = new RegisterCommand(
                Username: sanitizedUsername,
                Email: sanitizedEmail,
                Password: dto.Password
            );

            var result = await _authService.RegisterAsync(command);

            if (!result.IsSuccess)
                throw new ConflictException(result.ErrorMessage ?? ErrorMessages.RegistrationFailed);

            _logger.LogInformation("User registered successfully: {Username}", sanitizedUsername);
            return Ok(result);
        }
        catch (Exception ex) when (!(ex is ApiException))
        {
            _logger.LogError(ex, "Unexpected error during registration");
            throw;
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Map DTO → Command
        var command = new LoginCommand(
            UsernameOrEmail: dto.UsernameOrEmail,
            Password: dto.Password
        );

        var result = await _authService.LoginAsync(command);

        if (!result.IsSuccess)
            return Unauthorized(new { message = result.ErrorMessage });

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<object>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var newToken = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
            return Ok(new { token = newToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var token = GetTokenFromHeader();
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = ErrorMessages.TokenNotProvided });

        var success = await _authService.LogoutAsync(token);
        if (!success)
            return BadRequest(new { message = ErrorMessages.LogoutFailed });

        return Ok(new { message = ErrorMessages.LogoutSuccess });
    }

    [HttpPost("validate")]
    public async Task<ActionResult<object>> ValidateToken()
    {
        var token = GetTokenFromHeader();
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = ErrorMessages.TokenNotProvided, isValid = false });

        var isValid = await _authService.ValidateTokenAsync(token);
        return Ok(new { isValid });
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<PasswordResetResponse>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                throw new ValidationException(ErrorMessages.InvalidModelState, ModelState);

            // Sanitize input
            var sanitizedEmail = _validationService.SanitizeInput(dto.Email);

            // Validate email format
            if (!_validationService.IsValidEmail(sanitizedEmail))
                throw new ValidationException(ErrorMessages.InvalidEmailFormat);

            _logger.LogInformation("Password reset requested for email: {Email}", sanitizedEmail);

            // Map DTO → Command
            var command = new ForgotPasswordCommand(Email: sanitizedEmail);

            var result = await _authService.ForgotPasswordAsync(command);

            return Ok(result);
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

            // Validate token and password
            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest(new { Message = ErrorMessages.ResetTokenRequired });
            
            if (!_validationService.IsValidPassword(dto.NewPassword))
                throw new ValidationException(ErrorMessages.InvalidPasswordFormat);

            _logger.LogInformation("Password reset attempt with token: {Token}", dto.Token.Substring(0, Math.Min(8, dto.Token.Length)) + "...");

            // Map DTO → Command
            var command = new ResetPasswordCommand(
                Token: dto.Token,
                NewPassword: dto.NewPassword
            );

            var result = await _authService.ResetPasswordAsync(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
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

