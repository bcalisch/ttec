namespace GeoOps.Api.Models;

public class Attachment
{
    public Guid Id { get; set; }
    public AttachmentEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string BlobUri { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
}
