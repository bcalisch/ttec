using GeoOps.Api.Contracts.Attachments;
using GeoOps.Api.Data;
using GeoOps.Api.Models;
using GeoOps.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoOps.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly string _uploadsPath;

    public AttachmentsController(AppDbContext dbContext, ICurrentUserService currentUser, IWebHostEnvironment env)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _uploadsPath = Path.Combine(env.ContentRootPath, "uploads");
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AttachmentResponse>>> GetAttachments(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AttachmentEntityType>(entityType, true, out var parsedType))
        {
            return BadRequest("Invalid entityType.");
        }

        var attachments = await _dbContext.Attachments
            .AsNoTracking()
            .Where(x => x.EntityType == parsedType && x.EntityId == entityId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync(cancellationToken);

        var response = attachments.Select(ToResponse).ToList();
        return Ok(response);
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<ActionResult<AttachmentResponse>> Upload(
        [FromForm] string entityType,
        [FromForm] Guid entityId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AttachmentEntityType>(entityType, true, out var parsedType))
        {
            return BadRequest("Invalid entityType.");
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest("No file provided.");
        }

        Directory.CreateDirectory(_uploadsPath);

        var fileId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{fileId}{extension}";
        var filePath = Path.Combine(_uploadsPath, storedFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var attachment = new Attachment
        {
            Id = fileId,
            EntityType = parsedType,
            EntityId = entityId,
            BlobUri = storedFileName,
            ContentType = file.ContentType,
            UploadedBy = _currentUser.Email,
            UploadedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Attachments.Add(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(attachment));
    }

    [HttpGet("{id:guid}/file")]
    public async Task<ActionResult> DownloadFile(Guid id, CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.Attachments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (attachment is null)
        {
            return NotFound();
        }

        var filePath = Path.Combine(_uploadsPath, attachment.BlobUri);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(stream, attachment.ContentType);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.Attachments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (attachment is null)
        {
            return NotFound();
        }

        var filePath = Path.Combine(_uploadsPath, attachment.BlobUri);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _dbContext.Attachments.Remove(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private AttachmentResponse ToResponse(Attachment a) => new(
        a.Id,
        a.EntityType.ToString(),
        a.EntityId,
        a.ContentType,
        a.UploadedBy,
        a.UploadedAt,
        $"/api/attachments/{a.Id}/file"
    );
}
