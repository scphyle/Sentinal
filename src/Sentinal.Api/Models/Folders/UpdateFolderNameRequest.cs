using System.ComponentModel.DataAnnotations;

namespace Sentinal.Api.Models.Folders;

public class UpdateFolderNameRequest
{
    [Required]
    [MaxLength(255)]
    public required string NewName { get; set; }
}