using System.Security.Claims;
using Microsoft.Extensions.Options;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;

namespace MovieWatchlist.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository m_userRepository;
    private readonly IRefreshTokenRepository m_refreshTokenRepository;
    private readonly IPasswordResetTokenRepository m_passwordResetTokenRepository;
    private readonly IJwtTokenService m_jwtTokenService;
    private readonly JwtSettings m_jwtSettings;
    private readonly IEmailService m_emailService;
    private readonly IPasswordHasher m_passwordHasher;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings,
        IEmailService emailService,
        IPasswordHasher passwordHasher)
    {
        m_userRepository = userRepository;
        m_refreshTokenRepository = refreshTokenRepository;
        m_passwordResetTokenRepository = passwordResetTokenRepository;
        m_jwtTokenService = jwtTokenService;
        m_jwtSettings = jwtSettings.Value;
        m_emailService = emailService;
        m_passwordHasher = passwordHasher;
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterCommand command)
    {
        var usernameResult = Username.Create(command.Username);
        if (usernameResult.IsFailure)
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: usernameResult.Error
            );
        }

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: emailResult.Error
            );
        }

        var passwordResult = Password.Create(command.Password);
        if (passwordResult.IsFailure)
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: passwordResult.Error
            );
        }

        var existingUserByEmail = await m_userRepository.GetByEmailAsync(emailResult.Value!);
        if (existingUserByEmail != null)
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: ErrorMessages.EmailAlreadyRegistered
            );
        }

        var existingUserByUsername = await m_userRepository.GetByUsernameAsync(usernameResult.Value!);
        if (existingUserByUsername != null)
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: ErrorMessages.UsernameAlreadyTaken
            );
        }

        var user = User.Create(
            usernameResult.Value!,
            emailResult.Value!,
            m_passwordHasher.HashPassword(command.Password)
        );

        await m_userRepository.AddAsync(user);

        var token = m_jwtTokenService.GenerateToken(user);
        var refreshToken = m_jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntity = RefreshToken.Create(user.Id, refreshToken, m_jwtSettings.RefreshTokenExpirationDays);

        await m_refreshTokenRepository.AddAsync(refreshTokenEntity);

        return new AuthenticationResult(
            IsSuccess: true,
            Token: token,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(m_jwtSettings.ExpirationMinutes),
            User: new UserInfo(
                Id: user.Id,
                Username: user.Username.Value,
                Email: user.Email.Value,
                CreatedAt: user.CreatedAt
            )
        );
    }

    public async Task<AuthenticationResult> LoginAsync(LoginCommand command)
    {
        var user = await FindUserByUsernameOrEmailAsync(command.UsernameOrEmail);
        if (user == null)
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: ErrorMessages.InvalidCredentials
            );
        }

        if (!m_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: ErrorMessages.InvalidCredentials
            );
        }

        user.UpdateLastLogin();
        await m_userRepository.UpdateAsync(user);

        var token = m_jwtTokenService.GenerateToken(user);
        var refreshToken = m_jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntity = RefreshToken.Create(user.Id, refreshToken, m_jwtSettings.RefreshTokenExpirationDays);

        await m_refreshTokenRepository.AddAsync(refreshTokenEntity);

        return new AuthenticationResult(
            IsSuccess: true,
            Token: token,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(m_jwtSettings.ExpirationMinutes),
            User: new UserInfo(
                Id: user.Id,
                Username: user.Username.Value,
                Email: user.Email.Value,
                CreatedAt: user.CreatedAt
            )
        );
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        var principal = m_jwtTokenService.ValidateToken(token);
        return Task.FromResult(principal != null);
    }

    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        var token = await m_refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (token == null)
        {
            throw new UnauthorizedAccessException(ErrorMessages.InvalidOrExpiredRefreshToken);
        }

        var user = await m_userRepository.GetByIdAsync(token.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException(ErrorMessages.UserNotFound);
        }

        token.Revoke();
        await m_refreshTokenRepository.UpdateAsync(token);

        var newToken = m_jwtTokenService.GenerateToken(user);
        var newRefreshToken = m_jwtTokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = RefreshToken.Create(user.Id, newRefreshToken, m_jwtSettings.RefreshTokenExpirationDays);

        await m_refreshTokenRepository.AddAsync(newRefreshTokenEntity);

        return newToken;
    }

    public async Task<bool> LogoutAsync(string token)
    {
        try
        {
            var principal = m_jwtTokenService.ValidateToken(token);
            if (principal == null) return false;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return false;

            var userRefreshTokens = await m_refreshTokenRepository.GetActiveByUserIdAsync(userId);
            foreach (var refreshToken in userRefreshTokens)
            {
                refreshToken.Revoke();
                await m_refreshTokenRepository.UpdateAsync(refreshToken);
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

            var user = await m_userRepository.GetByEmailAsync(emailResult.Value!);

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

            var existingTokens = await m_passwordResetTokenRepository.GetUnusedByUserIdAsync(user.Id);
            foreach (var token in existingTokens)
            {
                token.MarkAsUsed();
                await m_passwordResetTokenRepository.UpdateAsync(token);
            }

            var passwordResetToken = PasswordResetToken.Create(user.Id, resetToken, ValidationConstants.Password.ResetTokenExpirationHours);

            await m_passwordResetTokenRepository.AddAsync(passwordResetToken);
    
            await m_emailService.SendPasswordResetEmailAsync(user.Email.Value, resetToken, user.Username.Value);

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
            var resetToken = await m_passwordResetTokenRepository.GetValidByTokenAsync(command.Token);

            if (resetToken == null)
            {
                return new PasswordResetResponse(
                    Success: false,
                    Message: ErrorMessages.InvalidOrExpiredResetToken
                );
            }

            // Find user
            var user = await m_userRepository.GetByIdAsync(resetToken.UserId);
            if (user == null)
            {
                return new PasswordResetResponse(
                    Success: false,
                    Message: ErrorMessages.UserNotFound
                );
            }

            var hashedPassword = m_passwordHasher.HashPassword(command.NewPassword);
            user.ChangePassword(hashedPassword);

            resetToken.MarkAsUsed();

            await m_userRepository.UpdateAsync(user);
            await m_passwordResetTokenRepository.UpdateAsync(resetToken);

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
            var user = await m_userRepository.GetByEmailAsync(emailResult.Value!);
            if (user != null) return user;
        }

        var usernameResult = Username.Create(usernameOrEmail);
        if (usernameResult.IsSuccess)
        {
            return await m_userRepository.GetByUsernameAsync(usernameResult.Value!);
        }

        return null;
    }
}

