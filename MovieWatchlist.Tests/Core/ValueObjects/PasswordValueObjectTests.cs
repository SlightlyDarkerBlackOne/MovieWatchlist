using FluentAssertions;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.ValueObjects;
using MovieWatchlist.Tests.Infrastructure;
using Xunit;

namespace MovieWatchlist.Tests.Core.ValueObjects;

public class PasswordValueObjectTests : UnitTestBase
{
    [Theory]
    [InlineData(TestConstants.Users.DefaultPassword)]
    [InlineData("MySecure1@Pass")]
    [InlineData("Test123$")]
    [InlineData("ComplexP@ss1")]
    [InlineData("Valid8!Pass")]
    public void Create_WithValidPasswords_ReturnsSuccess(string password)
    {
        // Act
        var result = Password.Create(password);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Value.Should().Be(password);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ReturnsFailure(string? password)
    {
        // Act
        var result = Password.Create(password!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ValidationConstants.Password.InvalidFormatMessage);
    }

    [Theory]
    [InlineData("short1@")]
    [InlineData("Pass1@")]
    public void Create_WithTooShortPassword_ReturnsFailure(string password)
    {
        // Act
        var result = Password.Create(password);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ValidationConstants.Password.InvalidFormatMessage);
    }

    [Fact]
    public void Create_WithTooLongPassword_ReturnsFailure()
    {
        // Arrange
        var longPassword = "A" + new string('a', 99) + "1!";

        // Act
        var result = Password.Create(longPassword);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ValidationConstants.Password.InvalidFormatMessage);
    }

    [Theory]
    [InlineData(TestConstants.Users.NoUppercasePassword)]
    [InlineData(TestConstants.Users.NoLowercasePassword)]
    [InlineData("Password")]
    [InlineData(TestConstants.Users.NoSpecialCharPassword)]
    [InlineData(TestConstants.Users.NoNumberPassword)]
    [InlineData(TestConstants.Users.TooShortPassword)]
    [InlineData(TestConstants.Users.NoSpecialCharLongPassword)]
    public void Create_WithInvalidPasswords_ReturnsFailure(string password)
    {
        // Act
        var result = Password.Create(password);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ToString_ReturnsMaskedValue()
    {
        // Arrange
        var password = Password.Create(TestConstants.Users.DefaultPassword).Value!;

        // Act
        var result = password.ToString();

        // Assert
        result.Should().Be(TestConstants.DisplayValues.MaskedPassword);
    }

    [Fact]
    public void Equals_WithSamePassword_ReturnsTrue()
    {
        // Arrange
        var password1 = Password.Create(TestConstants.Users.DefaultPassword).Value!;
        var password2 = Password.Create(TestConstants.Users.DefaultPassword).Value!;

        // Act
        var result = password1.Equals(password2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentPassword_ReturnsFalse()
    {
        // Arrange
        var password1 = Password.Create(TestConstants.Users.ValidPassword1).Value!;
        var password2 = Password.Create(TestConstants.Users.ValidPassword2).Value!;

        // Act
        var result = password1.Equals(password2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSamePassword_ReturnsSameHash()
    {
        // Arrange
        var password1 = Password.Create(TestConstants.Users.DefaultPassword).Value!;
        var password2 = Password.Create(TestConstants.Users.DefaultPassword).Value!;

        // Act
        var hash1 = password1.GetHashCode();
        var hash2 = password2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentPassword_ReturnsDifferentHash()
    {
        // Arrange
        var password1 = Password.Create(TestConstants.Users.ValidPassword1).Value!;
        var password2 = Password.Create(TestConstants.Users.ValidPassword2).Value!;

        // Act
        var hash1 = password1.GetHashCode();
        var hash2 = password2.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
    }
}

