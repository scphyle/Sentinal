using Sentinal.Domain.Folders;

namespace Sentinal.Domain.Files;

public class FileEntity
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid FolderId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool MarkedForDeletion { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? PreviousVersionId { get; set; }
    public bool IsPartOfHistory { get; set; }

    // Navigation properties
    public FolderEntity? Folder { get; set; }
    // public User? User { get; set; } // Future
    public FileEntity Copy()
    {
        return new FileEntity()
        {
            FileName = this.FileName,
            FileSize = this.FileSize,
            ContentType = this.ContentType,
            Description = this.Description,
            FolderId = this.FolderId,
            UserId = this.UserId,
            CreatedAt = this.CreatedAt,
            UpdatedAt = this.UpdatedAt,
            MarkedForDeletion = this.MarkedForDeletion,
            DeletedAt = this.DeletedAt,
            Folder = this.Folder,
        };
    }
}