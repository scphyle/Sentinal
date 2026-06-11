using Sentinal.Domain.Files;

namespace Sentinal.Domain.Folders;

public class FolderEntity
{
    public Guid Id { get; set; }
    public string FolderName { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool? MarkedForDeletion { get; set; }

    // Navigation properties
    public FolderEntity? Parent { get; set; }
    public ICollection<FolderEntity> Children { get; set; } = new List<FolderEntity>();
    public ICollection<FileEntity> Files { get; set; } = new List<FileEntity>();
    // public User? User { get; set; } // Future
}