using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Sentinal.Domain.Files;
using Sentinal.Domain.Folders;
using Sentinal.Domain.Users;

namespace Sentinal.Infrastructure.Common.Persistence;

public class SentinalDbContext : DbContext
{
    public DbSet<FileEntity> Files { get; set; } = null!;
    public DbSet<FolderEntity> Folders { get; set; } = null!;
    public DbSet<UserEntity> Users { get; set; } = null!;

    public SentinalDbContext(DbContextOptions<SentinalDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

}