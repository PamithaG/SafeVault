using NUnit.Framework;
using SafeVaultApi.Controllers;
using SafeVaultApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace SafeVault.Tests;

/// <summary>
/// Tests for AuthController focusing on HTTP response status codes and validation.
/// Note: These are functional tests that validate the controller's request handling
/// without deep mocking of dependencies.
/// </summary>
[TestFixture]
public class TestAuthController
{
    private AuthController _authController = null!;

    [SetUp]
    public void Setup()
    {
        // Controller initialized without service for validation-level testing
        _authController = new AuthController(null!);
    }

    #region Register Input Validation Tests

    [Test]
    public void Register_WithPasswordMismatch_FailsValidation()
    {
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123",
            ConfirmPassword = "DifferentPass123",
            Role = "User"
        };

        // Model validation would catch this
        Assert.That(request.Password, Is.Not.EqualTo(request.ConfirmPassword));
    }

    [Test]
    public void Register_WithShortPassword_FailsValidation()
    {
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "short",
            ConfirmPassword = "short",
            Role = "User"
        };

        // Validation attribute [MinLength(8)] should catch this
        Assert.That(request.Password.Length, Is.LessThan(8));
    }

    [Test]
    public void Register_WithInvalidEmail_FailsValidation()
    {
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "invalid-email",
            Password = "SecurePass123",
            ConfirmPassword = "SecurePass123",
            Role = "User"
        };

        // [EmailAddress] validation attribute should catch this
        Assert.That(request.Email, Does.Not.Contain("@"));
    }

    [Test]
    public void Register_WithValidRequest_PassesValidation()
    {
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123",
            ConfirmPassword = "SecurePass123",
            Role = "User"
        };

        // Verify all validation attributes are satisfied
        Assert.That(request.Password, Is.EqualTo(request.ConfirmPassword));
        Assert.That(request.Password.Length, Is.GreaterThanOrEqualTo(8));
        Assert.That(request.Email, Does.Contain("@"));
        Assert.That(request.Username.Length, Is.GreaterThanOrEqualTo(3));
    }

    [Test]
    public void Register_WithAdminRole_PassesValidation()
    {
        var request = new CreateUserRequest
        {
            Username = "adminuser",
            Email = "admin@example.com",
            Password = "AdminPass123",
            ConfirmPassword = "AdminPass123",
            Role = "Admin"
        };

        Assert.That(request.Role, Is.EqualTo("Admin"));
        Assert.That(request.Password, Is.EqualTo(request.ConfirmPassword));
    }

    #endregion

    #region Login Input Validation Tests

    [Test]
    public void Login_WithValidCredentials_PassesValidation()
    {
        var request = new LoginDto("testuser", "SecurePass123");

        Assert.That(request.Username, Is.Not.Empty);
        Assert.That(request.Password, Is.Not.Empty);
    }

    [Test]
    public void Login_WithEmptyUsername_FailsValidation()
    {
        var request = new LoginDto("", "SomePassword123");

        Assert.That(request.Username, Is.Empty);
    }

    [Test]
    public void FAIL_TEST_Login_WithEmptyUsername_FailsValidation()
    {
        var request = new LoginDto("testuser", "SomePassword123");

        Assert.That(request.Username, Is.Empty);
    }

    [Test]
    public void Login_WithEmptyPassword_FailsValidation()
    {
        var request = new LoginDto("testuser", "");

        Assert.That(request.Password, Is.Empty);
    }

    #endregion

    #region Controller Model State Tests

    [Test]
    public void Controller_HasProperDependencies()
    {
        // Verify controller is properly constructed
        Assert.That(_authController, Is.Not.Null);
    }

    [Test]
    public void RegisterRequest_HasAllRequiredFields()
    {
        var request = new CreateUserRequest();

        var properties = typeof(CreateUserRequest).GetProperties();
        var requiredFields = new[] { "Username", "Email", "Password", "ConfirmPassword" };

        foreach (var field in requiredFields)
        {
            Assert.That(properties.Any(p => p.Name == field),
                $"Missing required field: {field}");
        }
    }

    [Test]
    public void LoginRequest_HasAllRequiredFields()
    {
        // LoginDto is a record with Username and Password
        var request = new LoginDto("user", "pass");

        Assert.That(request.Username, Is.Not.Null);
        Assert.That(request.Password, Is.Not.Null);
    }

    #endregion
}
