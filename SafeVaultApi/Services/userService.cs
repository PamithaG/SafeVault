using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using SafeVaultApi.DTOs;
using SafeVaultApi.Models;
using SafeVaultApi.Repositores;
using SafeVaultApi.Util;

namespace SafeVaultApi.Services;

public class UserService
{
    private readonly UserRepository _repository;
    private readonly JwtSettings _jwtSettings;

    public UserService(UserRepository repository, IOptions<JwtSettings> jwtOptions)
    {
        _repository = repository;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<int> CreateUserAsync(CreateUserRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match.");

        var username = InputValidator.Sanitize(request.Username);
        var email = InputValidator.Sanitize(request.Email);

        if (!InputValidator.IsValidUsername(username))
            throw new ArgumentException("Invalid username");

        if (!InputValidator.IsValidEmail(email))
            throw new ArgumentException("Invalid email");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters.");

        var role = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? "Admin"
            : "User";

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
            Role = role
        };

        try
        {
            return await _repository.CreateUserAsync(user);
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            throw;
        }
    }

    public async Task<string> AuthenticateAsync(LoginDto request)
    {
        if (!InputValidator.IsValidUsername(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Invalid username or password");

        var user = await _repository.GetUserByUsernameAsync(request.Username);
        if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash))
            throw new ArgumentException("Invalid username or password");

        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("username", user.Username),
            new Claim("role", user.Role),
            new Claim("userId", user.UserID.ToString()),
            new Claim("email", user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
            signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
