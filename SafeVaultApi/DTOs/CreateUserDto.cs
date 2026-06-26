using System.ComponentModel.DataAnnotations;

namespace SafeVaultApi.DTOs;

public class CreateUserRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    // [RegularExpression(@"^[a-zA-Z0-9_-]+$")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords must match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [RegularExpression(@"^(Admin|User)$")]
    public string Role { get; set; } = "User";
}
