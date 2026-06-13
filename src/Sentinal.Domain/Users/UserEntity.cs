using System.Runtime.CompilerServices;
using Sentinal.Domain.Files;
using Sentinal.Domain.Folders;

namespace Sentinal.Domain.Users;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
    public bool MarkedForDeletion { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<FileEntity> Files { get; set; } = new List<FileEntity>();
    public ICollection<FolderEntity> Folders { get; set; } = new List<FolderEntity>();
}