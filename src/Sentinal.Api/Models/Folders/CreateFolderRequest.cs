using System.ComponentModel.DataAnnotations;

namespace Sentinal.Api.Models.Folders;

public class CreateFolderRequest
{
    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }
    public Guid? ParentId { get; set; }
}