using Sentinal.Domain.Folders;

namespace Sentinal.Domain.Files;

public class FileEntity
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public Guid FolderId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool? MarkedForDeletion { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public FolderEntity? Folder { get; set; }
    // public User? User { get; set; } // Future
}