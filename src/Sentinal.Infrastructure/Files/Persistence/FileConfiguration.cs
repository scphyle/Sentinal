using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinal.Domain.Files;

namespace Sentinal.Infrastructure.Files.Persistence;

public class FileConfiguration : IEntityTypeConfiguration<FileEntity>
{
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FolderId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.MarkedForDeletion).IsRequired();
        builder.Property(x => x.DeletedAt).IsRequired(false);

        // Relationships
        // Many files belong to one folder
        builder.HasOne(x => x.Folder)
            .WithMany(f => f.Files)
            .HasForeignKey(x => x.FolderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
