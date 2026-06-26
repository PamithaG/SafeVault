using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SafeVaultApi.DTOs;
using SafeVaultApi.Services;

namespace SafeVaultApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            int id = await _userService.CreateUserAsync(request);
            if (id > 0)
                return Ok(new { UserId = id });
            else
                return BadRequest("Failed to create user");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (MySqlException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult AdminDashboard()
    {
        return Ok(new { Message = "Welcome to the Admin Dashboard" });
    }
}
