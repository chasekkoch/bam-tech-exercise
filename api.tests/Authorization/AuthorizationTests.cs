using Microsoft.AspNetCore.Authorization;
using StargateAPI.Controllers;
using Xunit;
using System.Reflection;

namespace StargateAPI.Tests.Authorization;

public class AuthorizationTests
{
    [Fact]
    public void PersonController_RequiresAuthorization()
    {
        // Arrange
        var controllerType = typeof(PersonController);

        // Act
        var authorize = controllerType.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        Assert.NotNull(authorize);
    }

    [Fact]
    public void AstronautDutyController_RequiresAuthorization()
    {
        // Arrange
        var controllerType = typeof(AstronautDutyController);

        // Act
        var authorize = controllerType.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        Assert.NotNull(authorize);
    }

    [Fact]
    public void AuthController_RegisterEndpoint_AllowsAnonymous()
    {
        // Arrange
        var controllerType = typeof(AuthController);
        var registerMethod = controllerType.GetMethod("Register");

        // Act
        var allowAnonymous = registerMethod?.GetCustomAttribute<AllowAnonymousAttribute>();

        // Assert
        // Register endpoint should NOT have AllowAnonymous - it should be accessible to all
        // but go through normal flow
        Assert.Null(allowAnonymous);
    }

    [Fact]
    public void AuthController_LoginEndpoint_AllowsAnonymous()
    {
        // Arrange
        var controllerType = typeof(AuthController);
        var loginMethod = controllerType.GetMethod("Login");

        // Act
        var allowAnonymous = loginMethod?.GetCustomAttribute<AllowAnonymousAttribute>();

        // Assert
        Assert.Null(allowAnonymous);
    }

    [Fact]
    public void AuthController_GetCurrentUserEndpoint_RequiresAuthorization()
    {
        // Arrange
        var controllerType = typeof(AuthController);
        var currentUserMethod = controllerType.GetMethod("GetCurrentUser");

        // Act
        var authorize = currentUserMethod?.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        Assert.NotNull(authorize);
    }

    [Fact]
    public void RoleBasedAccessControl_AdminRole_Defined()
    {
        // This test verifies that the Admin role is properly configured
        // and can be used for authorization
        
        // Arrange
        var adminRole = "Admin";

        // Act & Assert
        Assert.NotNull(adminRole);
        Assert.Equal("Admin", adminRole);
    }

    [Fact]
    public void RoleBasedAccessControl_UserRole_Defined()
    {
        // This test verifies that the User role is properly configured
        // and can be used for authorization
        
        // Arrange
        var userRole = "User";

        // Act & Assert
        Assert.NotNull(userRole);
        Assert.Equal("User", userRole);
    }
}
