using Backend.Api.Contracts.Observations;
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
[Route("api/projects/{projectId:guid}/observations")]
public class ObservationsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ObservationsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ObservationResponse>>> GetObservations(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == projectId, cancellationToken);

        if (!projectExists)
        {
            return NotFound();
        }

        var observations = await _dbContext.Observations
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new ObservationResponse(
                x.Id,
                x.ProjectId,
                x.Timestamp,
                x.Location.X,
                x.Location.Y,
                x.Note,
                x.Tags))
            .ToListAsync(cancellationToken);

        return Ok(observations);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ObservationResponse>> GetObservation(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var observation = await _dbContext.Observations
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.Id == id)
            .Select(x => new ObservationResponse(
                x.Id,
                x.ProjectId,
                x.Timestamp,
                x.Location.X,
                x.Location.Y,
                x.Note,
                x.Tags))
            .FirstOrDefaultAsync(cancellationToken);

        if (observation is null)
        {
            return NotFound();
        }

        return Ok(observation);
    }

    [HttpPost]
    public async Task<ActionResult<ObservationResponse>> CreateObservation(
        Guid projectId,
        [FromBody] CreateObservationRequest request,
        CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == projectId, cancellationToken);

        if (!projectExists)
        {
            return NotFound();
        }

        var observation = new Observation
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Timestamp = request.Timestamp,
            Location = CreatePoint(request.Longitude, request.Latitude),
            Note = request.Note,
            Tags = request.Tags
        };

        _dbContext.Observations.Add(observation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new ObservationResponse(
            observation.Id,
            observation.ProjectId,
            observation.Timestamp,
            observation.Location.X,
            observation.Location.Y,
            observation.Note,
            observation.Tags);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ObservationResponse>> UpdateObservation(
        Guid projectId,
        Guid id,
        [FromBody] UpdateObservationRequest request,
        CancellationToken cancellationToken)
    {
        var observation = await _dbContext.Observations
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Id == id, cancellationToken);

        if (observation is null)
        {
            return NotFound();
        }

        observation.Timestamp = request.Timestamp;
        observation.Location = CreatePoint(request.Longitude, request.Latitude);
        observation.Note = request.Note;
        observation.Tags = request.Tags;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new ObservationResponse(
            observation.Id,
            observation.ProjectId,
            observation.Timestamp,
            observation.Location.X,
            observation.Location.Y,
            observation.Note,
            observation.Tags);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteObservation(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var observation = await _dbContext.Observations
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Id == id, cancellationToken);

        if (observation is null)
        {
            return NotFound();
        }

        _dbContext.Observations.Remove(observation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static Point CreatePoint(double longitude, double latitude)
    {
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        return geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
    }
}
