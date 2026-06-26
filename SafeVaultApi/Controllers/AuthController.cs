using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SafeVaultApi.DTOs;
using SafeVaultApi.Services;

namespace SafeVaultApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var id = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(Register), new { id }, new { UserId = id });
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

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var token = await _userService.AuthenticateAsync(request);
            return Ok(new { Token = token });
        }
        catch (ArgumentException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}
