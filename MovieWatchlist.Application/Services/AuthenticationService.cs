using System.Security.Claims;
using Microsoft.Extensions.Options;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;

namespace MovieWatchlist.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings,
        IEmailService emailService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<User>> RegisterUserAsync(RegisterCommand command)
    {
        var usernameResult = Username.Create(command.Username);
        if (usernameResult.IsFailure)
            return Result<User>.Failure(usernameResult.Error);

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result<User>.Failure(emailResult.Error);

        var passwordResult = Password.Create(command.Password);
        if (passwordResult.IsFailure)
            return Result<User>.Failure(passwordResult.Error);

        var existingUserByEmail = await _userRepository.GetByEmailAsync(emailResult.Value!);
        if (existingUserByEmail != null)
            return Result<User>.Failure(ErrorMessages.EmailAlreadyRegistered);

        var existingUserByUsername = await _userRepository.GetByUsernameAsync(usernameResult.Value!);
        if (existingUserByUsername != null)
            return Result<User>.Failure(ErrorMessages.UsernameAlreadyTaken);

        var user = User.Create(
            usernameResult.Value!,
            emailResult.Value!,
            _passwordHasher.HashPassword(command.Password)
        );

        await _userRepository.AddAsync(user);

        return Result<User>.Success(user);
    }

    public AuthenticationResult GenerateAuthenticationResult(User user)
    {
        var token = _jwtTokenService.GenerateToken(user);

        return new AuthenticationResult(
            IsSuccess: true,
            Token: token,
            RefreshToken: null,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            User: new UserInfo(
                Id: user.Id,
                Username: user.Username.Value,
                Email: user.Email.Value,
                CreatedAt: user.CreatedAt
            )
        );
    }

    public async Task<AuthenticationResult> GenerateAuthenticationResultWithRefreshTokenAsync(User user)
    {
        var token = _jwtTokenService.GenerateToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntity = RefreshToken.Create(user.Id, refreshToken, _jwtSettings.RefreshTokenExpirationDays);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        return new AuthenticationResult(
            IsSuccess: true,
            Token: token,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            User: new UserInfo(
                Id: user.Id,
                Username: user.Username.Value,
                Email: user.Email.Value,
                CreatedAt: user.CreatedAt
            )
        );
    }

    public async Task<Result<RefreshTokenResult>> CreateRefreshTokenAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<RefreshTokenResult>.Failure(ErrorMessages.UserNotFound);

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenEntity = RefreshToken.Create(
            userId,
            refreshToken,
            _jwtSettings.RefreshTokenExpirationDays
        );

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        return Result<RefreshTokenResult>.Success(new RefreshTokenResult(
            refreshToken,
            refreshTokenEntity.ExpiresAt
        ));
    }

    public async Task<Result<AuthenticationResult>> LoginAsync(LoginCommand command)
    {
        var user = await FindUserByUsernameOrEmailAsync(command.UsernameOrEmail);
        if (user == null)
        {
            return Result<AuthenticationResult>.Failure(ErrorMessages.InvalidCredentials);
        }

        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            return Result<AuthenticationResult>.Failure(ErrorMessages.InvalidCredentials);
        }

        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user);

        var authResult = await GenerateAuthenticationResultWithRefreshTokenAsync(user);

        return Result<AuthenticationResult>.Success(authResult);
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        var principal = _jwtTokenService.ValidateToken(token);
        return Task.FromResult(principal != null);
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (token == null)
        {
            throw new UnauthorizedAccessException(ErrorMessages.InvalidOrExpiredRefreshToken);
        }

        var user = await _userRepository.GetByIdAsync(token.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException(ErrorMessages.UserNotFound);
        }

        token.Revoke();
        await _refreshTokenRepository.UpdateAsync(token);

        var newToken = _jwtTokenService.GenerateToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = RefreshToken.Create(user.Id, newRefreshToken, _jwtSettings.RefreshTokenExpirationDays);

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);

        return new AuthenticationResult(
            IsSuccess: true,
            Token: newToken,
            RefreshToken: newRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            User: new UserInfo(
                Id: user.Id,
                Username: user.Username.Value,
                Email: user.Email.Value,
                CreatedAt: user.CreatedAt
            )
        );
    }

    public async Task<bool> LogoutAsync(string token)
    {
        try
        {
            var principal = _jwtTokenService.ValidateToken(token);
            if (principal == null) return false;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return false;

            var userRefreshTokens = await _refreshTokenRepository.GetActiveByUserIdAsync(userId);
            foreach (var refreshToken in userRefreshTokens)
            {
                refreshToken.Revoke();
                await _refreshTokenRepository.UpdateAsync(refreshToken);
            }
            

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PasswordResetResponse> ForgotPasswordAsync(ForgotPasswordCommand command)
    {
        try
        {
            var emailResult = Email.Create(command.Email);
            if (emailResult.IsFailure)
            {
                // Return success to prevent email enumeration attacks
                return new PasswordResetResponse(
                    Success: true,
                    Message: ErrorMessages.PasswordResetEmailSent
                );
            }

            var user = await _userRepository.GetByEmailAsync(emailResult.Value!);

            // Always return success to prevent email enumeration attacks
            if (user == null)
            {
                return new PasswordResetResponse(
                    Success: true,
                    Message: ErrorMessages.PasswordResetEmailSent
                );
            }

            var resetToken = Guid.NewGuid().ToString();
            var expiresAt = DateTime.UtcNow.AddHours(ValidationConstants.Password.ResetTokenExpirationHours);

            var existingTokens = await _passwordResetTokenRepository.GetUnusedByUserIdAsync(user.Id);
            foreach (var token in existingTokens)
            {
                token.MarkAsUsed();
                await _passwordResetTokenRepository.UpdateAsync(token);
            }

            var passwordResetToken = PasswordResetToken.Create(user.Id, resetToken, ValidationConstants.Password.ResetTokenExpirationHours);

            await _passwordResetTokenRepository.AddAsync(passwordResetToken);
    
            await _emailService.SendPasswordResetEmailAsync(user.Email.Value, resetToken, user.Username.Value);

            return new PasswordResetResponse(
                Success: true,
                Message: ErrorMessages.PasswordResetEmailSent
            );
        }
        catch (Exception)
        {
            return new PasswordResetResponse(
                Success: false,
                Message: ErrorMessages.PasswordResetRequestError
            );
        }
    }

    public async Task<PasswordResetResponse> ResetPasswordAsync(ResetPasswordCommand command)
    {
        try
        {
            var passwordResult = Password.Create(command.NewPassword);
            if (passwordResult.IsFailure)
            {
                return new PasswordResetResponse(
                    Success: false,
                    Message: passwordResult.Error
                );
            }

            // Find valid reset token
            var resetToken = await _passwordResetTokenRepository.GetValidByTokenAsync(command.Token);

            if (resetToken == null)
            {
                return new PasswordResetResponse(
                    Success: false,
                    Message: ErrorMessages.InvalidOrExpiredResetToken
                );
            }

            // Find user
            var user = await _userRepository.GetByIdAsync(resetToken.UserId);
            if (user == null)
            {
                return new PasswordResetResponse(
                    Success: false,
                    Message: ErrorMessages.UserNotFound
                );
            }

            var hashedPassword = _passwordHasher.HashPassword(command.NewPassword);
            user.ChangePassword(hashedPassword);

            resetToken.MarkAsUsed();

            await _userRepository.UpdateAsync(user);
            await _passwordResetTokenRepository.UpdateAsync(resetToken);

            return new PasswordResetResponse(
                Success: true,
                Message: ErrorMessages.PasswordResetSuccess
            );
        }
        catch (Exception)
        {
            return new PasswordResetResponse(
                Success: false,
                Message: ErrorMessages.PasswordResetFailed
            );
        }
    }

    private async Task<User?> FindUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        var emailResult = Email.Create(usernameOrEmail);
        if (emailResult.IsSuccess)
        {
            var user = await _userRepository.GetByEmailAsync(emailResult.Value!);
            if (user != null) return user;
        }

        var usernameResult = Username.Create(usernameOrEmail);
        if (usernameResult.IsSuccess)
        {
            return await _userRepository.GetByUsernameAsync(usernameResult.Value!);
        }

        return null;
    }
}

