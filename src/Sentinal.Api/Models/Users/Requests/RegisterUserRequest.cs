using System.ComponentModel.DataAnnotations;

namespace Sentinal.Api.Models.Requests;

public class RegisterUserRequest
{
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string Username { get; set; } = null!;
    [Required]
    [EmailAddress]
    [StringLength(255, MinimumLength = 7)]
    public string Email { get; set; } = null!;
    [Required]
    [StringLength(255, MinimumLength = 8)]
    public string Password { get; set; } = null!;
}