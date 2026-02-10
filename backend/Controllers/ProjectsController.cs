using Backend.Api.Contracts.Features;
using Backend.Api.Contracts.Projects;
using Backend.Api.Data;
using Backend.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProjectsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Project>>> GetProjects(CancellationToken cancellationToken)
    {
        var projects = await _dbContext.Projects
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Project>> GetProject(Guid id, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Client = request.Client,
            Status = request.Status,
            StartDate = request.StartDate is null ? null : DateOnly.FromDateTime(request.StartDate.Value),
            EndDate = request.EndDate is null ? null : DateOnly.FromDateTime(request.EndDate.Value)
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Project>> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (project is null)
        {
            return NotFound();
        }

        project.Name = request.Name;
        project.Client = request.Client;
        project.Status = request.Status;
        project.StartDate = request.StartDate is null ? null : DateOnly.FromDateTime(request.StartDate.Value);
        project.EndDate = request.EndDate is null ? null : DateOnly.FromDateTime(request.EndDate.Value);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(project);
    }

    [HttpPost("{id:guid}/boundaries")]
    public async Task<ActionResult<ProjectBoundary>> CreateBoundary(Guid id, [FromBody] CreateProjectBoundaryRequest request, CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects
            .AnyAsync(x => x.Id == id, cancellationToken);

        if (!projectExists)
        {
            return NotFound();
        }

        if (!TryReadGeoJsonGeometry(request.GeoJson, out var geometry, out var errorMessage))
        {
            return BadRequest(errorMessage ?? "Invalid GeoJSON.");
        }

        var boundary = new ProjectBoundary
        {
            Id = Guid.NewGuid(),
            ProjectId = id,
            Polygon = geometry
        };

        _dbContext.ProjectBoundaries.Add(boundary);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(boundary);
    }

    [HttpGet("{id:guid}/features")]
    public async Task<ActionResult<FeaturesResponse>> GetFeatures(
        Guid id,
        [FromQuery] string? bbox,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? types,
        CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == id, cancellationToken);

        if (!projectExists)
        {
            return NotFound();
        }

        var typeSet = ParseTypes(types);
        var bboxPolygon = TryParseBbox(bbox, out var parsedPolygon) ? parsedPolygon : null;

        if (bbox is not null && bboxPolygon is null)
        {
            return BadRequest("Invalid bbox. Use 'minLon,minLat,maxLon,maxLat'.");
        }

        var tests = new List<TestResultFeature>();
        var observations = new List<ObservationFeature>();
        var sensors = new List<SensorFeature>();

        if (typeSet.Contains("tests"))
        {
            var query = _dbContext.TestResults
                .AsNoTracking()
                .Where(x => x.ProjectId == id);

            if (from is not null)
            {
                query = query.Where(x => x.Timestamp >= from.Value);
            }

            if (to is not null)
            {
                query = query.Where(x => x.Timestamp <= to.Value);
            }

            if (bboxPolygon is not null)
            {
                query = query.Where(x => x.Location.Intersects(bboxPolygon));
            }

            tests = await query
                .OrderByDescending(x => x.Timestamp)
                .Select(x => new TestResultFeature(
                    x.Id,
                    x.TestTypeId,
                    x.TestType.Name,
                    x.TestType.Unit,
                    x.Timestamp,
                    x.Value,
                    x.Status.ToString(),
                    x.Location.X,
                    x.Location.Y))
                .ToListAsync(cancellationToken);
        }

        if (typeSet.Contains("obs"))
        {
            var query = _dbContext.Observations
                .AsNoTracking()
                .Where(x => x.ProjectId == id);

            if (from is not null)
            {
                query = query.Where(x => x.Timestamp >= from.Value);
            }

            if (to is not null)
            {
                query = query.Where(x => x.Timestamp <= to.Value);
            }

            if (bboxPolygon is not null)
            {
                query = query.Where(x => x.Location.Intersects(bboxPolygon));
            }

            observations = await query
                .OrderByDescending(x => x.Timestamp)
                .Select(x => new ObservationFeature(
                    x.Id,
                    x.Timestamp,
                    x.Note,
                    x.Tags,
                    x.Location.X,
                    x.Location.Y))
                .ToListAsync(cancellationToken);
        }

        if (typeSet.Contains("sensors"))
        {
            var query = _dbContext.Sensors
                .AsNoTracking()
                .Where(x => x.ProjectId == id);

            if (bboxPolygon is not null)
            {
                query = query.Where(x => x.Location.Intersects(bboxPolygon));
            }

            sensors = await query
                .OrderBy(x => x.Type)
                .Select(x => new SensorFeature(
                    x.Id,
                    x.Type,
                    x.Location.X,
                    x.Location.Y))
                .ToListAsync(cancellationToken);
        }

        var response = new FeaturesResponse(tests, observations, sensors);
        return Ok(response);
    }

    private static HashSet<string> ParseTypes(string? types)
    {
        if (string.IsNullOrWhiteSpace(types))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "tests", "obs", "sensors" };
        }

        var parsed = types
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(type => type.ToLowerInvariant())
            .ToHashSet();

        if (parsed.Count == 0)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "tests", "obs", "sensors" };
        }

        return parsed;
    }

    private static bool TryParseBbox(string? bbox, out Polygon? polygon)
    {
        polygon = null;

        if (string.IsNullOrWhiteSpace(bbox))
        {
            return false;
        }

        var parts = bbox.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!double.TryParse(parts[0], out var minLon) ||
            !double.TryParse(parts[1], out var minLat) ||
            !double.TryParse(parts[2], out var maxLon) ||
            !double.TryParse(parts[3], out var maxLat))
        {
            return false;
        }

        if (minLon >= maxLon || minLat >= maxLat)
        {
            return false;
        }

        var coordinates = new[]
        {
            new Coordinate(minLon, minLat),
            new Coordinate(maxLon, minLat),
            new Coordinate(maxLon, maxLat),
            new Coordinate(minLon, maxLat),
            new Coordinate(minLon, minLat)
        };

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        polygon = geometryFactory.CreatePolygon(coordinates);
        return true;
    }

    private static bool TryReadGeoJsonGeometry(string geoJson, out Geometry? geometry, out string? errorMessage)
    {
        geometry = null;
        errorMessage = null;

        try
        {
            using var document = JsonDocument.Parse(geoJson);
            var root = document.RootElement;

            if (root.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
            {
                var type = typeElement.GetString();
                if (string.Equals(type, "Feature", StringComparison.OrdinalIgnoreCase))
                {
                    if (!root.TryGetProperty("geometry", out var geometryElement))
                    {
                        errorMessage = "GeoJSON Feature is missing geometry.";
                        return false;
                    }

                    return TryReadGeometryObject(geometryElement, out geometry, out errorMessage);
                }

                return TryReadGeometryObject(root, out geometry, out errorMessage);
            }

            errorMessage = "GeoJSON missing type.";
            return false;
        }
        catch (JsonException)
        {
            errorMessage = "Invalid GeoJSON JSON.";
            return false;
        }
    }

    private static bool TryReadGeometryObject(JsonElement element, out Geometry? geometry, out string? errorMessage)
    {
        geometry = null;
        errorMessage = null;

        if (!element.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
        {
            errorMessage = "GeoJSON geometry missing type.";
            return false;
        }

        var type = typeElement.GetString();
        if (!element.TryGetProperty("coordinates", out var coordinatesElement))
        {
            errorMessage = "GeoJSON geometry missing coordinates.";
            return false;
        }

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
        {
            if (!TryReadPolygon(geometryFactory, coordinatesElement, out var polygon, out errorMessage))
            {
                return false;
            }

            geometry = polygon;
            return true;
        }

        if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase))
        {
            if (coordinatesElement.ValueKind != JsonValueKind.Array)
            {
                errorMessage = "MultiPolygon coordinates must be an array.";
                return false;
            }

            var polygons = new List<Polygon>();
            foreach (var polygonElement in coordinatesElement.EnumerateArray())
            {
                if (!TryReadPolygon(geometryFactory, polygonElement, out var polygon, out errorMessage))
                {
                    return false;
                }

                polygons.Add(polygon);
            }

            geometry = geometryFactory.CreateMultiPolygon(polygons.ToArray());
            return true;
        }

        errorMessage = "Boundary must be a Polygon or MultiPolygon.";
        return false;
    }

    private static bool TryReadPolygon(GeometryFactory geometryFactory, JsonElement coordinatesElement, out Polygon polygon, out string? errorMessage)
    {
        polygon = geometryFactory.CreatePolygon();
        errorMessage = null;

        if (coordinatesElement.ValueKind != JsonValueKind.Array)
        {
            errorMessage = "Polygon coordinates must be an array.";
            return false;
        }

        var rings = coordinatesElement.EnumerateArray().ToList();
        if (rings.Count == 0)
        {
            errorMessage = "Polygon coordinates must include at least one ring.";
            return false;
        }

        if (!TryReadRing(rings[0], out var exteriorRing, out errorMessage))
        {
            return false;
        }

        var holes = new List<LinearRing>();
        for (var i = 1; i < rings.Count; i++)
        {
            if (!TryReadRing(rings[i], out var hole, out errorMessage))
            {
                return false;
            }

            holes.Add(hole);
        }

        polygon = geometryFactory.CreatePolygon(exteriorRing, holes.ToArray());
        return true;
    }

    private static bool TryReadRing(JsonElement ringElement, out LinearRing ring, out string? errorMessage)
    {
        ring = default!;
        errorMessage = null;

        if (ringElement.ValueKind != JsonValueKind.Array)
        {
            errorMessage = "Linear ring must be an array.";
            return false;
        }

        var coordinates = new List<Coordinate>();
        foreach (var position in ringElement.EnumerateArray())
        {
            if (position.ValueKind != JsonValueKind.Array)
            {
                errorMessage = "Position must be an array of [lon, lat].";
                return false;
            }

            var values = position.EnumerateArray().ToList();
            if (values.Count < 2)
            {
                errorMessage = "Position must have at least [lon, lat].";
                return false;
            }

            if (!values[0].TryGetDouble(out var lon) || !values[1].TryGetDouble(out var lat))
            {
                errorMessage = "Position must contain numeric lon/lat.";
                return false;
            }

            coordinates.Add(new Coordinate(lon, lat));
        }

        if (coordinates.Count < 4)
        {
            errorMessage = "Linear ring must have at least 4 positions.";
            return false;
        }

        if (!coordinates.First().Equals2D(coordinates.Last()))
        {
            coordinates.Add(new Coordinate(coordinates.First().X, coordinates.First().Y));
        }

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        ring = geometryFactory.CreateLinearRing(coordinates.ToArray());
        return true;
    }
}
