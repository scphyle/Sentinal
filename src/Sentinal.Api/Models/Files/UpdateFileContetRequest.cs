using System.ComponentModel.DataAnnotations;

namespace Sentinal.Api.Models.Files;

public class UpdateFileContetRequest
{
    [Required]
    public Guid FileId { get; set; }
    [Required]
    [MaxLength(255)]
    public required string ContentType { get; set; }
    [Required]
    public required MemoryStream Stream { get; set; } 
}