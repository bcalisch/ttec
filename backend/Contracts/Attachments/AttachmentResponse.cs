using Backend.Api.Models;

namespace Backend.Api.Contracts.Attachments;

public record AttachmentResponse(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string ContentType,
    string UploadedBy,
    DateTimeOffset UploadedAt,
    string Url
);
