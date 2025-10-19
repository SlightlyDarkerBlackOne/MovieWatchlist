using System.Security.Claims;
using Microsoft.Extensions.Options;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository m_userRepository;
    private readonly IRefreshTokenRepository m_refreshTokenRepository;
    private readonly IPasswordResetTokenRepository m_passwordResetTokenRepository;
    private readonly IJwtTokenService m_jwtTokenService;
    private readonly IUnitOfWork m_unitOfWork;
    private readonly JwtSettings m_jwtSettings;
    private readonly IEmailService m_emailService;
    private readonly IPasswordHasher m_passwordHasher;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        IEmailService emailService,
        IPasswordHasher passwordHasher)
    {
        m_userRepository = userRepository;
        m_refreshTokenRepository = refreshTokenRepository;
        m_passwordResetTokenRepository = passwordResetTokenRepository;
        m_jwtTokenService = jwtTokenService;
        m_unitOfWork = unitOfWork;
        m_jwtSettings = jwtSettings.Value;
        m_emailService = emailService;
        m_passwordHasher = passwordHasher;
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterCommand command)
    {
        // Check if user already exists
        var existingUserByEmail = await m_userRepository.GetByEmailAsync(command.Email);
        if (existingUserByEmail != null)
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: ErrorMessages.EmailAlreadyRegistered
            );
        }

        var existingUserByUsername = await m_userRepository.GetByUsernameAsync(command.Username);
        if (existingUserByUsername != null)
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: ErrorMessages.UsernameAlreadyTaken
            );
        }

        // Create new user using factory method
        var user = User.Create(
            command.Username,
            command.Email,
            m_passwordHasher.HashPassword(command.Password)
        );

        await m_userRepository.AddAsync(user);
        await m_unitOfWork.SaveChangesAsync();

        // Generate tokens
        var token = m_jwtTokenService.GenerateToken(user);
        var refreshToken = m_jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntity = RefreshToken.Create(user.Id, refreshToken, m_jwtSettings.RefreshTokenExpirationDays);

        await m_refreshTokenRepository.AddAsync(refreshTokenEntity);
        await m_unitOfWork.SaveChangesAsync();

        return new AuthenticationResult(
            IsSuccess: true,
            Token: token,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(m_jwtSettings.ExpirationMinutes),
            User: new UserInfo(
                Id: user.Id,
                Username: user.Username,
                Email: user.Email,
                CreatedAt: user.CreatedAt
            )
        );
    }

    public async Task<AuthenticationResult> LoginAsync(LoginCommand command)
    {
        // Find user by username or email
        var user = await FindUserByUsernameOrEmailAsync(command.UsernameOrEmail);
        if (user == null)
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: ErrorMessages.InvalidCredentials
            );
        }

        // Verify password
        if (!m_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            return new AuthenticationResult(
                IsSuccess: false,
                ErrorMessage: ErrorMessages.InvalidCredentials
            );
        }

        // Update last login using domain method
        user.UpdateLastLogin();
        await m_userRepository.UpdateAsync(user);
        await m_unitOfWork.SaveChangesAsync();

        // Generate tokens
        var token = m_jwtTokenService.GenerateToken(user);
        var refreshToken = m_jwtTokenService.GenerateRefreshToken();

        var refreshTokenEntity = RefreshToken.Create(user.Id, refreshToken, m_jwtSettings.RefreshTokenExpirationDays);

        await m_refreshTokenRepository.AddAsync(refreshTokenEntity);
        await m_unitOfWork.SaveChangesAsync();

        return new AuthenticationResult(
            IsSuccess: true,
            Token: token,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(m_jwtSettings.ExpirationMinutes),
            User: new UserInfo(
                Id: user.Id,
                Username: user.Username,
                Email: user.Email,
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
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var user = await m_userRepository.GetByIdAsync(token.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        token.Revoke();
        await m_refreshTokenRepository.UpdateAsync(token);

        var newToken = m_jwtTokenService.GenerateToken(user);
        var newRefreshToken = m_jwtTokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = RefreshToken.Create(user.Id, newRefreshToken, m_jwtSettings.RefreshTokenExpirationDays);

        await m_refreshTokenRepository.AddAsync(newRefreshTokenEntity);
        await m_unitOfWork.SaveChangesAsync();

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
            
            await m_unitOfWork.SaveChangesAsync();

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
            var user = await m_userRepository.GetByEmailAsync(command.Email);

            // Always return success to prevent email enumeration attacks
            var response = new PasswordResetResponse(
                Success: true,
                Message: "If the email exists, a password reset link has been sent."
            );

            if (user == null)
            {
                return response;
            }

            var resetToken = Guid.NewGuid().ToString();
            var expiresAt = DateTime.UtcNow.AddHours(1);

            var existingTokens = await m_passwordResetTokenRepository.GetUnusedByUserIdAsync(user.Id);
            foreach (var token in existingTokens)
            {
                token.MarkAsUsed();
                await m_passwordResetTokenRepository.UpdateAsync(token);
            }

            var passwordResetToken = PasswordResetToken.Create(user.Id, resetToken, 1); // 1 hour expiration

            await m_passwordResetTokenRepository.AddAsync(passwordResetToken);
            await m_unitOfWork.SaveChangesAsync();
    
            await m_emailService.SendPasswordResetEmailAsync(user.Email, resetToken, user.Username);

            return response;
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

            // Hash new password and update using domain method
            var hashedPassword = m_passwordHasher.HashPassword(command.NewPassword);
            user.ChangePassword(hashedPassword);

            resetToken.MarkAsUsed();

            await m_userRepository.UpdateAsync(user);
            await m_passwordResetTokenRepository.UpdateAsync(resetToken);
            await m_unitOfWork.SaveChangesAsync();

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
        var user = await m_userRepository.GetByUsernameAsync(usernameOrEmail);
        if (user != null) return user;
        return await m_userRepository.GetByEmailAsync(usernameOrEmail);
    }
}

