using System.Globalization;
using System.Text;
using Backend.Api.Data;
using Backend.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/export")]
public class ExportController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ExportController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("csv")]
    public async Task<ActionResult> ExportCsv(
        Guid projectId,
        [FromQuery] string? bbox,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] Guid? testTypeId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = BuildFilteredQuery(projectId, bbox, from, to, testTypeId, status);
        if (query is null)
        {
            return BadRequest("Invalid bbox format. Use 'minLon,minLat,maxLon,maxLat'.");
        }

        var results = await query
            .Include(x => x.TestType)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Id,ProjectId,TestTypeId,TestTypeName,Unit,Longitude,Latitude,Timestamp,Value,Status,Source,Technician");

        foreach (var r in results)
        {
            sb.AppendLine(string.Join(",",
                r.Id,
                r.ProjectId,
                r.TestTypeId,
                CsvEscape(r.TestType.Name),
                CsvEscape(r.TestType.Unit),
                r.Location.X.ToString(CultureInfo.InvariantCulture),
                r.Location.Y.ToString(CultureInfo.InvariantCulture),
                r.Timestamp.ToString("o"),
                r.Value.ToString(CultureInfo.InvariantCulture),
                r.Status,
                CsvEscape(r.Source),
                CsvEscape(r.Technician)));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "test-results.csv");
    }

    [HttpGet("geojson")]
    public async Task<ActionResult> ExportGeoJson(
        Guid projectId,
        [FromQuery] string? bbox,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] Guid? testTypeId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = BuildFilteredQuery(projectId, bbox, from, to, testTypeId, status);
        if (query is null)
        {
            return BadRequest("Invalid bbox format. Use 'minLon,minLat,maxLon,maxLat'.");
        }

        var results = await query
            .Include(x => x.TestType)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);

        var features = results.Select(r => new
        {
            type = "Feature",
            geometry = new
            {
                type = "Point",
                coordinates = new[] { r.Location.X, r.Location.Y }
            },
            properties = new
            {
                r.Id,
                r.ProjectId,
                r.TestTypeId,
                TestTypeName = r.TestType.Name,
                Unit = r.TestType.Unit,
                r.Timestamp,
                r.Value,
                Status = r.Status.ToString(),
                r.Source,
                r.Technician
            }
        });

        var featureCollection = new
        {
            type = "FeatureCollection",
            features
        };

        return new JsonResult(featureCollection)
        {
            ContentType = "application/geo+json"
        };
    }

    private IQueryable<TestResult>? BuildFilteredQuery(
        Guid projectId,
        string? bbox,
        DateTimeOffset? from,
        DateTimeOffset? to,
        Guid? testTypeId,
        string? status)
    {
        var query = _dbContext.TestResults
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId);

        if (!string.IsNullOrWhiteSpace(bbox))
        {
            var parts = bbox.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
                return null;

            if (!double.TryParse(parts[0], CultureInfo.InvariantCulture, out var minLon) ||
                !double.TryParse(parts[1], CultureInfo.InvariantCulture, out var minLat) ||
                !double.TryParse(parts[2], CultureInfo.InvariantCulture, out var maxLon) ||
                !double.TryParse(parts[3], CultureInfo.InvariantCulture, out var maxLat))
                return null;

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var bboxPolygon = geometryFactory.CreatePolygon(new[]
            {
                new Coordinate(minLon, minLat),
                new Coordinate(maxLon, minLat),
                new Coordinate(maxLon, maxLat),
                new Coordinate(minLon, maxLat),
                new Coordinate(minLon, minLat)
            });

            query = query.Where(x => x.Location.Intersects(bboxPolygon));
        }

        if (from is not null)
        {
            query = query.Where(x => x.Timestamp >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(x => x.Timestamp <= to.Value);
        }

        if (testTypeId is not null)
        {
            query = query.Where(x => x.TestTypeId == testTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TestStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        return query;
    }

    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
