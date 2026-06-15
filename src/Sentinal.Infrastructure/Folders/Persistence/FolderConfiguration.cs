using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinal.Domain.Folders;

namespace Sentinal.Infrastructure.Folders.Persistence;

public class FolderConfiguration : IEntityTypeConfiguration<FolderEntity>
{
    public void Configure(EntityTypeBuilder<FolderEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.FolderName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ParentFolderId).IsRequired(false);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.MarkedForDeletion).IsRequired();
        builder.Property(x => x.DeletedAt).IsRequired(false);
        builder.Property(x => x.FolderType).IsRequired(false);

        // Relationships
        // Self-referencing: Parent-Child folder hierarchy
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentFolderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // One folder has many files
        builder.HasMany(x => x.Files)
            .WithOne(f => f.Folder)
            .HasForeignKey(f => f.FolderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}