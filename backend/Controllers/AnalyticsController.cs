using Backend.Api.Data;
using Backend.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AnalyticsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("out-of-spec")]
    public async Task<ActionResult> GetOutOfSpec(Guid projectId, CancellationToken cancellationToken)
    {
        var results = await _dbContext.TestResults
            .AsNoTracking()
            .Include(x => x.TestType)
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        var outOfSpec = results
            .Where(r =>
            {
                if (r.TestType.MinThreshold is not null && r.Value < r.TestType.MinThreshold)
                    return true;
                if (r.TestType.MaxThreshold is not null && r.Value > r.TestType.MaxThreshold)
                    return true;
                return false;
            })
            .Select(r =>
            {
                decimal severityPercent = 0;
                if (r.TestType.MinThreshold is not null && r.Value < r.TestType.MinThreshold)
                {
                    severityPercent = r.TestType.MinThreshold.Value == 0
                        ? 100
                        : Math.Round(Math.Abs(r.TestType.MinThreshold.Value - r.Value) / r.TestType.MinThreshold.Value * 100, 2);
                }
                else if (r.TestType.MaxThreshold is not null && r.Value > r.TestType.MaxThreshold)
                {
                    severityPercent = r.TestType.MaxThreshold.Value == 0
                        ? 100
                        : Math.Round(Math.Abs(r.Value - r.TestType.MaxThreshold.Value) / r.TestType.MaxThreshold.Value * 100, 2);
                }

                return new
                {
                    r.Id,
                    r.TestTypeId,
                    TestTypeName = r.TestType.Name,
                    r.Value,
                    Unit = r.TestType.Unit,
                    r.TestType.MinThreshold,
                    r.TestType.MaxThreshold,
                    r.Status,
                    SeverityPercent = severityPercent,
                    Longitude = r.Location.X,
                    Latitude = r.Location.Y,
                    r.Timestamp
                };
            })
            .OrderByDescending(x => x.SeverityPercent)
            .ToList();

        return Ok(outOfSpec);
    }

    [HttpGet("coverage")]
    public async Task<ActionResult> GetCoverage(Guid projectId, CancellationToken cancellationToken)
    {
        var boundaries = await _dbContext.ProjectBoundaries
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        if (boundaries.Count == 0)
        {
            return Ok(new { cells = Array.Empty<object>(), message = "No boundaries defined for this project." });
        }

        // Compute the envelope of all boundaries
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (var b in boundaries)
        {
            var env = b.Polygon.EnvelopeInternal;
            if (env.MinX < minX) minX = env.MinX;
            if (env.MinY < minY) minY = env.MinY;
            if (env.MaxX > maxX) maxX = env.MaxX;
            if (env.MaxY > maxY) maxY = env.MaxY;
        }

        const int gridSize = 10;
        double cellWidth = (maxX - minX) / gridSize;
        double cellHeight = (maxY - minY) / gridSize;

        var testResults = await _dbContext.TestResults
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .Select(x => new { x.Location.X, x.Location.Y })
            .ToListAsync(cancellationToken);

        var cells = new List<CoverageCell>();
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                double cellMinX = minX + col * cellWidth;
                double cellMinY = minY + row * cellHeight;
                double cellMaxX = cellMinX + cellWidth;
                double cellMaxY = cellMinY + cellHeight;

                int count = testResults.Count(r =>
                    r.X >= cellMinX && r.X < cellMaxX &&
                    r.Y >= cellMinY && r.Y < cellMaxY);

                cells.Add(new CoverageCell(row, col, cellMinX, cellMinY, cellMaxX, cellMaxY, count));
            }
        }

        return Ok(new
        {
            gridSize,
            cells,
            gaps = cells.Count(c => c.Count == 0)
        });
    }

    [HttpGet("trends")]
    public async Task<ActionResult> GetTrends(
        Guid projectId,
        [FromQuery] string interval = "day",
        CancellationToken cancellationToken = default)
    {
        var results = await _dbContext.TestResults
            .AsNoTracking()
            .Include(x => x.TestType)
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.Timestamp)
            .ToListAsync(cancellationToken);

        var grouped = results
            .GroupBy(r =>
            {
                var dt = r.Timestamp.UtcDateTime;
                if (string.Equals(interval, "week", StringComparison.OrdinalIgnoreCase))
                {
                    // Group by ISO week start (Monday)
                    var dayOfWeek = (int)dt.DayOfWeek;
                    var monday = dt.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
                    return new { Period = monday.ToString("yyyy-MM-dd"), TestTypeId = r.TestTypeId, TestTypeName = r.TestType.Name };
                }
                return new { Period = dt.ToString("yyyy-MM-dd"), TestTypeId = r.TestTypeId, TestTypeName = r.TestType.Name };
            })
            .Select(g => new
            {
                g.Key.Period,
                g.Key.TestTypeId,
                g.Key.TestTypeName,
                Avg = Math.Round(g.Average(x => x.Value), 2),
                Min = g.Min(x => x.Value),
                Max = g.Max(x => x.Value),
                Count = g.Count()
            })
            .OrderBy(x => x.Period)
            .ThenBy(x => x.TestTypeName)
            .ToList();

        return Ok(grouped);
    }

    private record CoverageCell(int Row, int Col, double MinLon, double MinLat, double MaxLon, double MaxLat, int Count);
}
