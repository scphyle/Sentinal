using System.ComponentModel.DataAnnotations;

namespace Sentinal.Api.Models.Requests;

public class UpdateUsernameRequest
{
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string NewUsername { get; set; } = null!;
}