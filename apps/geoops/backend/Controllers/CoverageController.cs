using GeoOps.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoOps.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/coverage")]
public class CoverageController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CoverageController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
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

        var cells = new List<object>();
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

                cells.Add(new { row, col, minLon = cellMinX, minLat = cellMinY, maxLon = cellMaxX, maxLat = cellMaxY, count });
            }
        }

        return Ok(new { gridSize, cells, gaps = cells.Cast<dynamic>().Count(c => c.count == 0) });
    }
}
