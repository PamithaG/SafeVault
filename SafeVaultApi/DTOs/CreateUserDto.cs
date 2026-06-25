using System.ComponentModel.DataAnnotations;

namespace SafeVaultApi.DTOs;

public class CreateUserRequest
{
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}