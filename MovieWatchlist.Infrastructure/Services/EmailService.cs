using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Infrastructure.Services;

/// <summary>
/// Email service implementation for development/testing
/// In production, this would integrate with actual email providers like SendGrid, AWS SES, etc.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string username)
    {
        try
        {
            // In development, we'll just log the email content
            // In production, you would integrate with an actual email service
            // Frontend URL - adjust based on your environment
            var resetUrl = $"http://localhost:3000?token={resetToken}";
            
            var emailContent = $@"
Password Reset Request

Hello {username},

You have requested to reset your password for your MovieWatchlist account.

Click the link below to reset your password:
{resetUrl}

This link will expire in 1 hour.

If you did not request this password reset, please ignore this email.

Best regards,
MovieWatchlist Team
";

            _logger.LogInformation("========================================");
            _logger.LogInformation("PASSWORD RESET EMAIL (Development Mode)");
            _logger.LogInformation("========================================");
            _logger.LogInformation("To: {Email}", email);
            _logger.LogInformation("Username: {Username}", username);
            _logger.LogInformation("");
            _logger.LogInformation("Reset URL (copy and paste in browser):");
            _logger.LogInformation("{ResetUrl}", resetUrl);
            _logger.LogInformation("");
            _logger.LogInformation("Full Email Content:");
            _logger.LogInformation("{EmailContent}", emailContent);
            _logger.LogInformation("========================================");

            // Simulate async email sending
            await Task.Delay(100);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            return false;
        }
    }
}

