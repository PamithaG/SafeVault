using MySqlConnector;
using SafeVaultApi.DTOs;
using SafeVaultApi.Models;
using SafeVaultApi.Repositores;
using SafeVaultApi.Util;

namespace SafeVaultApi.Services;

public class UserService
{
    private readonly UserRepository _repository;

    public UserService(UserRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CreateUserAsync(
        CreateUserRequest request)
    {
        var user = new User
        {
            Username = InputValidator.Sanitize(request.Username),
            Email = InputValidator.Sanitize(request.Email)
        };
        try
        {
            if (!InputValidator.IsValidUsername(user.Username))
            {
                throw new ArgumentException("Invalid username");
            }
            if (!InputValidator.IsValidEmail(user.Email))
            {
                throw new ArgumentException("Invalid email");
            }
        
            return await _repository.CreateUserAsync(user);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException(ex.Message);
        }
        catch(MySqlException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            throw;
        }

    }
}