namespace Sentinal.Api.Models.Files;

public class UpdateFileDataRequest
{
    public Guid FileId { get; set; }
    public string? NewName { get; set; }
    public string? NewDescription { get; set; }
}