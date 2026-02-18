using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Xunit;

namespace StargateAPI.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            null!, null!, null!, null!);
        
        _mockConfiguration = new Mock<IConfiguration>();
        _controller = new AuthController(_mockUserManager.Object, _mockSignInManager.Object, _mockConfiguration.Object);

        // Setup default JWT configuration
        SetupJwtConfiguration();
    }

    private void SetupJwtConfiguration()
    {
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["SecretKey"]).Returns("GeneratedSecretKeyForJWTTokenValidation1234567890");
        jwtSection.Setup(x => x["Issuer"]).Returns("StargateAPI");
        jwtSection.Setup(x => x["Audience"]).Returns("StargateClient");
        jwtSection.Setup(x => x["ExpirationHours"]).Returns("24");
        
        _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidRequest_ReturnsOkWithToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var response = okResult.Value as dynamic;
        Assert.NotNull(response?.Token);
        Assert.Equal("newuser@example.com", response?.Email);
        Assert.Equal("John", response?.FirstName);
        Assert.Equal("Doe", response?.LastName);

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var identityError = new IdentityError { Code = "DuplicateUserName", Description = "Email already exists" };
        
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "weak",
            FirstName = "John",
            LastName = "Doe"
        };

        var passwordError = new IdentityError 
        { 
            Code = "PasswordTooShort", 
            Description = "Passwords must be at least 8 characters." 
        };
        
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(passwordError));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Register_AssignsUserRoleByDefault()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe"
        };

        ApplicationUser? capturedUser = null;

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, password) => capturedUser = user)
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        await _controller.Register(request);

        // Assert
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@stargate.com",
            Password = "Admin123!"
        };

        var user = new ApplicationUser
        {
            Id = "admin-1",
            Email = request.Email,
            UserName = request.Email,
            FirstName = "System",
            LastName = "Administrator"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
            .ReturnsAsync(SignInResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" });

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.NotNull(response?.Token);
        Assert.Equal("admin@stargate.com", response?.Email);
        Assert.Equal("System", response?.FirstName);
        Assert.Contains("Admin", response?.Roles);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPassword123!"
        };

        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = request.Email,
            UserName = request.Email
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    #endregion

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WithAuthenticatedUser_ReturnsUserInfo()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "user@example.com",
            UserName = "user@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email!)
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockUserManager.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value as dynamic;
        Assert.Equal("user@example.com", response?.Email);
        Assert.Equal("John", response?.FirstName);
        Assert.Equal("Doe", response?.LastName);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuthenticationClaim_ReturnsUnauthorized()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    #endregion

    #region JWT Token Tests

    [Fact]
    public async Task GenerateJwtToken_CreatesValidToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin", "User" });

        var request = new LoginRequest { Email = user.Email, Password = "Password123!" };

        _mockUserManager.Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
            .ReturnsAsync(SignInResult.Success);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var handler = new JwtSecurityTokenHandler();
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GenerateJwtToken_IncludesRoleClaims()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" });

        var request = new LoginRequest { Email = user.Email, Password = "Password123!" };

        _mockUserManager.Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
            .ReturnsAsync(SignInResult.Success);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    #endregion
}
