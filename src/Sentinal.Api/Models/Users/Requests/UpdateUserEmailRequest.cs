using System.ComponentModel.DataAnnotations;

namespace Sentinal.Api.Models.Requests;

public class UpdateUserEmailRequest
{
    [Required]
    [EmailAddress]
    [StringLength(255, MinimumLength = 7)]
    public string NewEmail { get; set; } = null!;
}