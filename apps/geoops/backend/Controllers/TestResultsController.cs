using GeoOps.Api.Contracts.TestResults;
using GeoOps.Api.Data;
using GeoOps.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace GeoOps.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}")]
public class TestResultsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public TestResultsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("test-results")]
    public async Task<ActionResult<TestResult>> CreateTestResult(
        Guid projectId,
        [FromBody] CreateTestResultRequest request,
        CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == projectId, cancellationToken);

        if (!projectExists)
        {
            return NotFound();
        }

        var testType = await _dbContext.TestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TestTypeId, cancellationToken);

        if (testType is null)
        {
            return BadRequest("Unknown testTypeId.");
        }

        var testResult = new TestResult
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            TestTypeId = request.TestTypeId,
            Timestamp = request.Timestamp,
            Value = request.Value,
            Status = ResolveStatus(request.Status, testType, request.Value),
            Source = request.Source ?? string.Empty,
            Technician = request.Technician ?? string.Empty,
            Location = CreatePoint(request.Longitude, request.Latitude)
        };

        _dbContext.TestResults.Add(testResult);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new TestResultResponse(
            testResult.Id,
            testResult.ProjectId,
            testResult.TestTypeId,
            testResult.Timestamp,
            testResult.Value,
            testResult.Status.ToString(),
            testResult.Location.X,
            testResult.Location.Y,
            testResult.Source,
            testResult.Technician
        );

        return Ok(response);
    }

    [HttpPost("ingest/test-results")]
    public async Task<ActionResult<object>> BatchIngest(
        Guid projectId,
        [FromBody] BatchIngestTestResultsRequest request,
        CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == projectId, cancellationToken);

        if (!projectExists)
        {
            return NotFound();
        }

        var existingBatch = await _dbContext.IngestBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.IdempotencyKey == request.IdempotencyKey, cancellationToken);

        if (existingBatch is not null)
        {
            return Ok(new { accepted = 0, duplicate = true, batchId = existingBatch.Id });
        }

        var testTypeIds = request.Items.Select(x => x.TestTypeId).Distinct().ToList();
        var testTypes = await _dbContext.TestTypes
            .Where(x => testTypeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (testTypes.Count != testTypeIds.Count)
        {
            return BadRequest("One or more testTypeId values are invalid.");
        }

        var results = request.Items.Select(item => new TestResult
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            TestTypeId = item.TestTypeId,
            Timestamp = item.Timestamp,
            Value = item.Value,
            Status = ResolveStatus(item.Status, testTypes[item.TestTypeId], item.Value),
            Source = item.Source ?? string.Empty,
            Technician = item.Technician ?? string.Empty,
            Location = CreatePoint(item.Longitude, item.Latitude)
        }).ToList();

        var ingestBatch = new IngestBatch
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            IdempotencyKey = request.IdempotencyKey,
            ReceivedAt = DateTimeOffset.UtcNow,
            ItemCount = results.Count
        };

        _dbContext.IngestBatches.Add(ingestBatch);
        _dbContext.TestResults.AddRange(results);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { accepted = results.Count, duplicate = false, batchId = ingestBatch.Id });
    }

    [HttpPost("test-results:import")]
    public async Task<ActionResult<object>> ImportCsv(
        Guid projectId,
        [FromBody] CsvImportRequest request,
        CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == projectId, cancellationToken);

        if (!projectExists)
        {
            return NotFound();
        }

        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            Actor = User?.Identity?.Name ?? "system",
            Action = "csv_import_queued",
            EntityType = "Project",
            EntityId = projectId,
            Timestamp = DateTimeOffset.UtcNow,
            MetadataJson = $"{{\"fileName\":\"{request.FileName}\",\"blobUri\":\"{request.BlobUri}\"}}"
        };

        _dbContext.AuditLogs.Add(audit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Accepted(new { status = "queued", fileName = request.FileName });
    }

    private static Point CreatePoint(double longitude, double latitude)
    {
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        return geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
    }

    private static TestStatus ResolveStatus(string? status, TestType testType, decimal value)
    {
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TestStatus>(status, true, out var parsed))
        {
            return parsed;
        }

        if (testType.MinThreshold.HasValue && value < testType.MinThreshold.Value)
        {
            return TestStatus.Fail;
        }

        if (testType.MaxThreshold.HasValue && value > testType.MaxThreshold.Value)
        {
            return TestStatus.Fail;
        }

        return TestStatus.Pass;
    }
}
