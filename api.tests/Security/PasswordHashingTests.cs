using Microsoft.AspNetCore.Identity;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Security;

public class PasswordHashingTests
{
    private readonly PasswordHasher<ApplicationUser> _hasher;

    public PasswordHashingTests()
    {
        _hasher = new PasswordHasher<ApplicationUser>();
    }

    [Fact]
    public void HashPassword_CreatesValidHash()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };
        var password = "SecurePassword123!";

        // Act
        var hash = _hasher.HashPassword(user, password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(password, hash); // Hash should not be plaintext
    }

    [Fact]
    public void VerifyHashedPassword_WithCorrectPassword_ReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };
        var password = "SecurePassword123!";
        var hash = _hasher.HashPassword(user, password);

        // Act
        var result = _hasher.VerifyHashedPassword(user, hash, password);

        // Assert
        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void VerifyHashedPassword_WithWrongPassword_ReturnsFailed()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _hasher.HashPassword(user, password);

        // Act
        var result = _hasher.VerifyHashedPassword(user, hash, wrongPassword);

        // Assert
        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void VerifyHashedPassword_WithEmptyPassword_ReturnsFailed()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };
        var password = "SecurePassword123!";
        var hash = _hasher.HashPassword(user, password);

        // Act
        var result = _hasher.VerifyHashedPassword(user, hash, "");

        // Assert
        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void HashPassword_DifferentHashesForSamePassword()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };
        var password = "SecurePassword123!";

        // Act
        var hash1 = _hasher.HashPassword(user, password);
        var hash2 = _hasher.HashPassword(user, password);

        // Assert
        Assert.NotEqual(hash1, hash2); // Each hash should be different due to salt
        // Both hashes should verify correctly with the password
        Assert.Equal(PasswordVerificationResult.Success, 
            _hasher.VerifyHashedPassword(user, hash1, password));
        Assert.Equal(PasswordVerificationResult.Success, 
            _hasher.VerifyHashedPassword(user, hash2, password));
    }

    [Fact]
    public void AdminUserPassword_CanBeVerified()
    {
        // Arrange
        var adminUser = new ApplicationUser
        {
            Id = "admin-user-id",
            Email = "admin@stargate.com",
            FirstName = "System",
            LastName = "Administrator"
        };
        var adminPassword = "Admin123!";

        // Act
        var hash = _hasher.HashPassword(adminUser, adminPassword);
        var verificationResult = _hasher.VerifyHashedPassword(adminUser, hash, adminPassword);

        // Assert
        Assert.Equal(PasswordVerificationResult.Success, verificationResult);
    }

    [Theory]
    [InlineData("ValidPassword123!")]
    [InlineData("AnotherValid123!")]
    [InlineData("ComplexP@ssw0rd")]
    public void HashPassword_WithValidPasswords_VerifiesSuccessfully(string password)
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };

        // Act
        var hash = _hasher.HashPassword(user, password);
        var result = _hasher.VerifyHashedPassword(user, hash, password);

        // Assert
        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Theory]
    [InlineData("ValidPassword123!", "WrongPassword123!")]
    [InlineData("Password1!", "Password2!")]
    [InlineData("SuperSecret123!", "supersecret123!")]
    public void VerifyHashedPassword_WithWrongPasswords_ReturnsFailed(string correctPassword, string wrongPassword)
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };
        var hash = _hasher.HashPassword(user, correctPassword);

        // Act
        var result = _hasher.VerifyHashedPassword(user, hash, wrongPassword);

        // Assert
        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void VerifyHashedPassword_IsCaseSensitive()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };
        var password = "SecurePassword123!";
        var hash = _hasher.HashPassword(user, password);

        // Act
        var result = _hasher.VerifyHashedPassword(user, hash, password.ToLower());

        // Assert
        Assert.Equal(PasswordVerificationResult.Failed, result);
    }
}
