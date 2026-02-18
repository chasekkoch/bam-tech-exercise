using Microsoft.AspNetCore.Identity;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests.Fixtures;

/// <summary>
/// Test data builder for creating test users and roles
/// </summary>
public class TestDataBuilder
{
    public static ApplicationUser CreateTestUser(
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        string id = "test-user-id")
    {
        return new ApplicationUser
        {
            Id = id,
            Email = email ?? "testuser@example.com",
            UserName = email ?? "testuser@example.com",
            FirstName = firstName ?? "Test",
            LastName = lastName ?? "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ApplicationUser CreateAdminUser()
    {
        return new ApplicationUser
        {
            Id = "admin-user-id",
            Email = "admin@stargate.com",
            UserName = "admin@stargate.com",
            FirstName = "System",
            LastName = "Administrator",
            CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    public static IdentityRole CreateAdminRole()
    {
        return new IdentityRole
        {
            Id = "1",
            Name = "Admin",
            NormalizedName = "ADMIN"
        };
    }

    public static IdentityRole CreateUserRole()
    {
        return new IdentityRole
        {
            Id = "2",
            Name = "User",
            NormalizedName = "USER"
        };
    }
}
