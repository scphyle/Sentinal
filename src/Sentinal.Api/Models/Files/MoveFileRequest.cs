namespace Sentinal.Api.Models.Files;

public class MoveFileRequest
{
    public Guid FileId { get; set; }
    public Guid DestinationFolderId { get; set; }
}