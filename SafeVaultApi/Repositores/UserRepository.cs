using MySqlConnector;
using SafeVaultApi.Models;

namespace SafeVaultApi.Repositores;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<int> CreateUserAsync(User user)
    {
        try
        {
            const string sql = @"
            INSERT INTO Users
            (
                Username,
                Email,
                PasswordHash,
                Role
            )
            VALUES
            (
                @Username,
                @Email,
                @PasswordHash,
                @Role
            );

            SELECT LAST_INSERT_ID();
            ";

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@Role", user.Role);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (MySqlException e)
        {
            Console.WriteLine($"MySqlException: {e.Number} - {e.Message}");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<User?> GetUserAsync(int id)
    {
        const string sql = @"
            SELECT UserID,
                   Username,
                   Email,
                   PasswordHash,
                   Role
            FROM Users
            WHERE UserID = @Id
        ";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? new User
            {
                UserID = reader.GetInt32("UserID"),
                Username = reader.GetString("Username"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                Role = reader.GetString("Role")
            }
            : null;
    }

    public async Task<User?> GetUserByUsernameAsync(
    string username)
    {
        const string sql = @"
        SELECT UserID,
               Username,
               Email,
               PasswordHash,
               Role
        FROM Users
        WHERE Username = @Username";

        await using var connection = new MySqlConnection(_connectionString);

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.Add(
            "@Username",
            MySqlDbType.VarChar,
            100).Value = username;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            UserID = reader.GetInt32(0),
            Username = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            Role = reader.GetString(4)
        };
    }
}