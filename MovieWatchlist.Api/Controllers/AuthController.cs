using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Core.DTOs;
using MovieWatchlist.Core.Exceptions;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Validation;

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
    public async Task<ActionResult<AuthenticationResult>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid model state", ModelState);

            // Sanitize inputs
            registerDto.Username = _validationService.SanitizeInput(registerDto.Username);
            registerDto.Email = _validationService.SanitizeInput(registerDto.Email);

            // Validate inputs
            var validationResult = _validationService.ValidateRegistrationInput(
                registerDto.Username, registerDto.Email, registerDto.Password);

            if (!validationResult.IsValid)
                throw new ValidationException("Validation failed", validationResult.Errors);

            _logger.LogInformation("Registration attempt for user: {Username}", registerDto.Username);

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.IsSuccess)
                throw new ConflictException(result.ErrorMessage ?? "Registration failed");

            _logger.LogInformation("User registered successfully: {Username}", registerDto.Username);
            return Ok(result);
        }
        catch (Exception ex) when (!(ex is ApiException))
        {
            _logger.LogError(ex, "Unexpected error during registration");
            throw;
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(loginDto);

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
            return BadRequest(new { message = "Token not provided" });

        var success = await _authService.LogoutAsync(token);
        if (!success)
            return BadRequest(new { message = "Failed to logout" });

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("validate")]
    public async Task<ActionResult<object>> ValidateToken()
    {
        var token = GetTokenFromHeader();
        if (string.IsNullOrEmpty(token))
            return BadRequest(new { message = "Token not provided", isValid = false });

        var isValid = await _authService.ValidateTokenAsync(token);
        return Ok(new { isValid });
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<PasswordResetResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid model state", ModelState);

            // Sanitize input
            forgotPasswordDto.Email = _validationService.SanitizeInput(forgotPasswordDto.Email);

            // Validate email format
            if (!_validationService.IsValidEmail(forgotPasswordDto.Email))
                throw new ValidationException("Invalid email format");

            _logger.LogInformation("Password reset requested for email: {Email}", forgotPasswordDto.Email);

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

            return Ok(result);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            return StatusCode(500, new { message = "An error occurred while processing your request." });
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<PasswordResetResponseDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
                throw new ValidationException("Invalid model state", ModelState);

            // Validate new password
            if (!_validationService.IsValidPassword(resetPasswordDto.NewPassword))
                throw new ValidationException("Invalid password format");

            _logger.LogInformation("Password reset attempt with token: {Token}", resetPasswordDto.Token?.Substring(0, Math.Min(8, resetPasswordDto.Token?.Length ?? 0)) + "...");

            var result = await _authService.ResetPasswordAsync(resetPasswordDto);

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
            return StatusCode(500, new { message = "An error occurred while resetting your password." });
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

