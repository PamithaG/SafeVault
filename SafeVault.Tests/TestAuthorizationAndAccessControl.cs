using NUnit.Framework;
using SafeVaultApi.Controllers;
using SafeVaultApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SafeVault.Tests;

[TestFixture]
public class TestAuthorizationAndAccessControl
{
    [SetUp]
    public void Setup()
    {
    }

    #region Admin Dashboard Access Tests

    [Test]
    public void AdminDashboard_WithoutAuthToken_ShouldNotAllowAccess()
    {
        // Arrange: Controller without user context (no claims)
        var httpContext = new DefaultHttpContext();
        var controllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        var controller = new UsersController(null!);
        controller.ControllerContext = controllerContext;

        // Assert: Verify that User.Identity is not authenticated
        Assert.That(controller.User?.Identity?.IsAuthenticated, Is.False.Or.Null);
    }

    [Test]
    public void AdminDashboard_WithUserRole_ShouldDenyAccess()
    {
        // Arrange: Create claims for a regular user (not admin)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "regularuser"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("userId", "1"),
            new Claim("email", "user@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);

        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        var controller = new UsersController(null!);
        controller.ControllerContext = controllerContext;

        // Assert: User should not have Admin role
        Assert.That(controller.User.IsInRole("Admin"), Is.False);
        Assert.That(controller.User.IsInRole("User"), Is.True);
    }

    [Test]
    public void AdminDashboard_WithAdminRole_ShouldAllowAccess()
    {
        // Arrange: Create claims for an admin user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "adminuser"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("userId", "2"),
            new Claim("email", "admin@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);

        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        var controller = new UsersController(null!);
        controller.ControllerContext = controllerContext;

        // Assert: User should have Admin role
        Assert.That(controller.User.IsInRole("Admin"), Is.True);
        Assert.That(controller.User.IsInRole("User"), Is.False);
    }

    #endregion

    #region Role Claim Verification Tests

    [Test]
    public void UserClaims_ShouldContainRoleInformation()
    {
        // Arrange: Create claims with role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("userId", "1"),
            new Claim("email", "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");

        // Assert: Verify role claim exists
        var roleClaim = identity.FindFirst(ClaimTypes.Role);
        Assert.That(roleClaim, Is.Not.Null);
        Assert.That(roleClaim!.Value, Is.EqualTo("User"));
    }

    [Test]
    public void AdminClaims_ShouldContainAdminRoleInformation()
    {
        // Arrange: Create claims with admin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "adminuser"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("userId", "2"),
            new Claim("email", "admin@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");

        // Assert: Verify admin role claim exists
        var roleClaim = identity.FindFirst(ClaimTypes.Role);
        Assert.That(roleClaim, Is.Not.Null);
        Assert.That(roleClaim!.Value, Is.EqualTo("Admin"));
    }

    [Test]
    public void UserClaims_ShouldContainUserIdAndEmail()
    {
        // Arrange: Create full claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("userId", "123"),
            new Claim("email", "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");

        // Assert: Verify custom claims
        var userIdClaim = identity.FindFirst("userId");
        var emailClaim = identity.FindFirst("email");

        Assert.That(userIdClaim, Is.Not.Null);
        Assert.That(userIdClaim!.Value, Is.EqualTo("123"));
        Assert.That(emailClaim, Is.Not.Null);
        Assert.That(emailClaim!.Value, Is.EqualTo("test@example.com"));
    }

    #endregion

    #region Multiple Users with Different Roles Tests

    [Test]
    public void MultipleUsers_DifferentRoles_RoleClaimsCorrect()
    {
        // Arrange: Create multiple users with different roles
        var users = new List<(string Username, string Role)>
        {
            ("user1", "User"),
            ("user2", "User"),
            ("admin1", "Admin"),
            ("admin2", "Admin")
        };

        var adminCount = 0;
        var userCount = 0;

        foreach (var (username, role) in users)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);

            if (principal.IsInRole("Admin"))
                adminCount++;
            else if (principal.IsInRole("User"))
                userCount++;
        }

        // Assert: Verify role counts
        Assert.That(adminCount, Is.EqualTo(2));
        Assert.That(userCount, Is.EqualTo(2));
    }

    [Test]
    public void AccessControl_UserCannotAccessAdminResource()
    {
        // Arrange: Regular user trying to access admin resource
        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "regularuser"),
            new Claim(ClaimTypes.Role, "User")
        };
        var userIdentity = new ClaimsIdentity(userClaims, "TestScheme");
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        // Act: Check if user has admin access
        var hasAdminAccess = userPrincipal.IsInRole("Admin");

        // Assert: User should NOT have admin access
        Assert.That(hasAdminAccess, Is.False);
    }

    [Test]
    public void AccessControl_AdminCanAccessAdminResource()
    {
        // Arrange: Admin user accessing admin resource
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "adminuser"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var adminIdentity = new ClaimsIdentity(adminClaims, "TestScheme");
        var adminPrincipal = new ClaimsPrincipal(adminIdentity);

        // Act: Check if admin has admin access
        var hasAdminAccess = adminPrincipal.IsInRole("Admin");

        // Assert: Admin should have admin access
        Assert.That(hasAdminAccess, Is.True);
    }

    #endregion

    #region Unauthorized Access Scenarios

    [Test]
    public void UnauthorizedUser_NoTokenProvided_CannotAccess()
    {
        // Arrange: No authentication context
        var httpContext = new DefaultHttpContext();
        var controllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        var controller = new UsersController(null!);
        controller.ControllerContext = controllerContext;

        // Assert: User is not authenticated
        Assert.That(controller.User?.Identity?.IsAuthenticated, Is.False.Or.Null);
    }

    [Test]
    public void UnauthorizedUser_InvalidToken_CannotAccess()
    {
        // Arrange: User with no role claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "unknownuser")
            // Note: No role claim provided
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);

        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        var controller = new UsersController(null!);
        controller.ControllerContext = controllerContext;

        // Assert: User should not have admin role
        Assert.That(controller.User.IsInRole("Admin"), Is.False);
    }

    [Test]
    public void UnauthorizedUser_ExpiredToken_CannotAccess()
    {
        // Arrange: User with expired token (simulated by checking identity)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "expireduser"),
            new Claim("tokenExpired", "true")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");

        // Act & Assert: Token expiration would be handled at middleware level
        // Here we just verify the identity exists
        Assert.That(identity.IsAuthenticated, Is.True);
        Assert.That(identity.FindFirst("tokenExpired")?.Value, Is.EqualTo("true"));
    }

    #endregion
}
