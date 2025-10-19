using FluentAssertions;
using MovieWatchlist.Application.Validation;
using MovieWatchlist.Tests.Infrastructure;
using Xunit;

namespace MovieWatchlist.Tests.Services;

/// <summary>
/// Unit tests for InputValidationService
/// </summary>
public class InputValidationServiceTests : UnitTestBase
{
    private readonly InputValidationService _validationService;

    public InputValidationServiceTests()
    {
        _validationService = new InputValidationService();
    }

    #region Email Validation Tests

    [Theory]
    [InlineData(TestConstants.Users.DefaultEmail)]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+tag@example.org")]
    [InlineData("user123@test-domain.com")]
    public void IsValidEmail_WithValidEmails_ReturnsTrue(string email)
    {
        // Act
        var result = _validationService.IsValidEmail(email);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    [InlineData("user@example")]
    [InlineData("user@.example.com")]
    public void IsValidEmail_WithInvalidEmails_ReturnsFalse(string? email)
    {
        // Act
        var result = _validationService.IsValidEmail(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidEmail_WithEmailTooLong_ReturnsFalse()
    {
        // Arrange
        var longEmail = new string('a', 90) + "@" + new string('b', 10) + ".com"; // 105 characters

        // Act
        var result = _validationService.IsValidEmail(longEmail);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Username Validation Tests

    [Theory]
    [InlineData(TestConstants.Users.DefaultUsername)]
    [InlineData("user123")]
    [InlineData("user_name")]
    [InlineData("user-name")]
    [InlineData("User123_Name")]
    [InlineData("abc")] // Minimum length
    [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwx")] // Maximum length (48 chars)
    public void IsValidUsername_WithValidUsernames_ReturnsTrue(string username)
    {
        // Act
        var result = _validationService.IsValidUsername(username);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")] // Too short
    [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyza")] // Too long (49 chars)
    [InlineData("user name")] // Contains space
    [InlineData("user.name")] // Contains dot
    [InlineData("user@name")] // Contains @
    [InlineData("user+name")] // Contains +
    public void IsValidUsername_WithInvalidUsernames_ReturnsFalse(string? username)
    {
        // Act
        var result = _validationService.IsValidUsername(username);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Password Validation Tests

    [Theory]
    [InlineData(TestConstants.Users.DefaultPassword)]
    [InlineData("MySecure1@Pass")]
    [InlineData("Test123$")]
    [InlineData("ComplexP@ss1")]
    public void IsValidPassword_WithValidPasswords_ReturnsTrue(string password)
    {
        // Act
        var result = _validationService.IsValidPassword(password);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("password")] // No uppercase, number, or special char
    [InlineData("PASSWORD")] // No lowercase, number, or special char
    [InlineData("Password")] // No number or special char
    [InlineData("Password1")] // No special char
    [InlineData("Password!")] // No number
    [InlineData("Pass1!")] // Too short
    [InlineData("Password123")] // No special char
    public void IsValidPassword_WithInvalidPasswords_ReturnsFalse(string? password)
    {
        // Act
        var result = _validationService.IsValidPassword(password);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidPassword_WithPasswordTooLong_ReturnsFalse()
    {
        // Arrange
        var longPassword = "A" + new string('a', 99) + "1!"; // 102 characters

        // Act
        var result = _validationService.IsValidPassword(longPassword);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Input Sanitization Tests

    [Theory]
    [InlineData("normal text", "normal text")]
    [InlineData("  whitespace  ", "whitespace")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void SanitizeInput_WithNormalText_ReturnsSanitizedText(string? input, string expected)
    {
        // Act
        var result = _validationService.SanitizeInput(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>", "&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;")]
    [InlineData("He said \"Hello\"", "He said &quot;Hello&quot;")]
    [InlineData("A & B", "A &amp; B")]
    [InlineData("Price: $10 & $20", "Price: $10 &amp; $20")]
    [InlineData("Test < > \" ' &", "Test &lt; &gt; &quot; &#39; &amp;")]
    public void SanitizeInput_WithSpecialCharacters_ReturnsSanitizedText(string input, string expected)
    {
        // Act
        var result = _validationService.SanitizeInput(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Registration Validation Tests

    [Fact]
    public void ValidateRegistrationInput_WithValidInput_ReturnsValidResult()
    {
        // Arrange
        var username = TestConstants.Users.DefaultUsername;
        var email = TestConstants.Users.DefaultEmail;
        var password = TestConstants.Users.DefaultPassword;

        // Act
        var result = _validationService.ValidateRegistrationInput(username, email, password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateRegistrationInput_WithInvalidUsername_ReturnsInvalidResult()
    {
        // Arrange
        var username = "ab"; // Too short
        var email = TestConstants.Users.DefaultEmail;
        var password = TestConstants.Users.DefaultPassword;

        // Act
        var result = _validationService.ValidateRegistrationInput(username, email, password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Username must be 3-50 characters long and contain only letters, numbers, underscores, and hyphens");
    }

    [Fact]
    public void ValidateRegistrationInput_WithInvalidEmail_ReturnsInvalidResult()
    {
        // Arrange
        var username = TestConstants.Users.DefaultUsername;
        var email = "invalid-email";
        var password = TestConstants.Users.DefaultPassword;

        // Act
        var result = _validationService.ValidateRegistrationInput(username, email, password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Email must be a valid format and not exceed 100 characters");
    }

    [Fact]
    public void ValidateRegistrationInput_WithInvalidPassword_ReturnsInvalidResult()
    {
        // Arrange
        var username = TestConstants.Users.DefaultUsername;
        var email = TestConstants.Users.DefaultEmail;
        var password = "weak";

        // Act
        var result = _validationService.ValidateRegistrationInput(username, email, password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password must be at least 8 characters with uppercase, lowercase, number, and special character");
    }

    [Fact]
    public void ValidateRegistrationInput_WithAllInvalidInputs_ReturnsMultipleErrors()
    {
        // Arrange
        var username = "ab";
        var email = "invalid";
        var password = "weak";

        // Act
        var result = _validationService.ValidateRegistrationInput(username, email, password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain("Username must be 3-50 characters long and contain only letters, numbers, underscores, and hyphens");
        result.Errors.Should().Contain("Email must be a valid format and not exceed 100 characters");
        result.Errors.Should().Contain("Password must be at least 8 characters with uppercase, lowercase, number, and special character");
    }

    #endregion
}
