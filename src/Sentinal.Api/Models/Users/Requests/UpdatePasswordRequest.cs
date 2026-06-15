using System.ComponentModel.DataAnnotations;

namespace Sentinal.Api.Models.Requests;

public class UpdatePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = null!;
    [Required]
    [StringLength(255, MinimumLength = 8)]
    public string NewPassword { get; set; } = null!;
}