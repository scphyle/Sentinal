using System.ComponentModel.DataAnnotations;

namespace Sentinal.Api.Models.Files;

public class CreateFileRequest()
{
    [Required]
    [MaxLength(1000)]
    public required string FileName { get; set; }
    [Required]
    [MaxLength(255)]
    public required string ContentType { get; set; }
    [Required]
    public required IFormFile File { get; set; } 
    /// <summary>
    /// If none is provided place in the users root directory (the userId)
    /// </summary>
    public Guid FolderId { get; set; }
    public string? Description { get; set; }
};