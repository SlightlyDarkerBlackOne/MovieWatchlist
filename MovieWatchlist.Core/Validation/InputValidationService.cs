using System.Net;
using System.Text.RegularExpressions;

namespace MovieWatchlist.Core.Validation;

public interface IInputValidationService
{
    bool IsValidEmail(string? email);
    bool IsValidUsername(string? username);
    bool IsValidPassword(string? password);
    string SanitizeInput(string? input);
    ValidationResult ValidateRegistrationInput(string username, string email, string password);
}

public class InputValidationService : IInputValidationService
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9]([a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UsernameRegex = new(
        @"^[a-zA-Z0-9_-]{3,50}$",
        RegexOptions.Compiled);

    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        RegexOptions.Compiled);

    public bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return EmailRegex.IsMatch(email) && email.Length <= 100;
    }

    public bool IsValidUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        return UsernameRegex.IsMatch(username);
    }

    public bool IsValidPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        return PasswordRegex.IsMatch(password) && password.Length <= 100;
    }

    public string SanitizeInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        
        return WebUtility.HtmlEncode(input.Trim());
    }

    public ValidationResult ValidateRegistrationInput(string username, string email, string password)
    {
        var errors = new List<string>();

        if (!IsValidUsername(username))
            errors.Add("Username must be 3-50 characters long and contain only letters, numbers, underscores, and hyphens");

        if (!IsValidEmail(email))
            errors.Add("Email must be a valid format and not exceed 100 characters");

        if (!IsValidPassword(password))
            errors.Add("Password must be at least 8 characters with uppercase, lowercase, number, and special character");

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
