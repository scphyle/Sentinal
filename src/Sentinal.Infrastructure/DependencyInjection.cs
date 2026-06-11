using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Common.Persistence;
using Sentinal.Infrastructure.Common.Security;
using Sentinal.Infrastructure.Files.Persistence;
using Sentinal.Infrastructure.Folders.Persistence;
using Sentinal.Infrastructure.Options;
using Sentinal.Infrastructure.Services;
using Sentinal.Infrastructure.Users.Persistence;

namespace Sentinal.Infrastructure;

public static class DependencyInjection
{

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SentinalDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, Argon2PasswordService>();

        services.AddScoped<IFileStorageService>(provider =>
        {
            var config = provider.GetRequiredService<IOptions<FileStorageOptions>>();
            return config.Value.StorageProvider switch
            {
                StorageType.Local => new LocalFileStorageService(config),
                StorageType.AwsS3 => new S3FileStorageService(config),
                StorageType.AzureBlob => new AzureBlobFileStorageService(config),
                _ => throw new InvalidOperationException("Invalid storage type")
            };
        });
        return services;
    }

}
