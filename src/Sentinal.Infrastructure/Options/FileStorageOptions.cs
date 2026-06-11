namespace Sentinal.Infrastructure.Options;

public class FileStorageOptions
{
    /// <summary>
    /// Gets or sets the storage provider type (Local, AwsS3, or AzureBlob).
    /// </summary>
    public StorageType StorageProvider { get; set; } = StorageType.Local;

    /// <summary>
    /// Gets or sets the maximum file size allowed in bytes.
    /// Default is 5 GB (5,368,709,120 bytes).
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 5_368_709_120; // 5 GB

    /// <summary>
    /// Gets or sets the base path for local file storage.
    /// </summary>
    public string BasePath { get; set; } = "./Sentinal";

    /// <summary>
    /// Gets or sets the number of days soft-deleted files are retained before permanent deletion.
    /// </summary>
    public int DeletedFileRetentionDays { get; set; } = 7;

    //Aws s3
    public string AwsAccessKey { get; set; } = string.Empty;
    public string AwsSecretKey { get; set; } = string.Empty;
    public string AwsRegion { get; set; } = string.Empty;
    public string AwsBucketName { get; set; } = string.Empty;

    //Azure blob storage
    public string AzureConnectionString { get; set; } = string.Empty;
    public string AzureContainerName { get; set; } = string.Empty;

}