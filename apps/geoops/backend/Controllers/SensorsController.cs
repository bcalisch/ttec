using GeoOps.Api.Contracts.Sensors;
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
[Route("api/projects/{projectId:guid}/sensors")]
public class SensorsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public SensorsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SensorResponse>>> GetSensors(
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

        var sensors = await _dbContext.Sensors
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.Type)
            .Select(x => new SensorResponse(
                x.Id,
                x.ProjectId,
                x.Type,
                x.Location.X,
                x.Location.Y,
                x.MetadataJson))
            .ToListAsync(cancellationToken);

        return Ok(sensors);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SensorResponse>> GetSensor(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var sensor = await _dbContext.Sensors
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.Id == id)
            .Select(x => new SensorResponse(
                x.Id,
                x.ProjectId,
                x.Type,
                x.Location.X,
                x.Location.Y,
                x.MetadataJson))
            .FirstOrDefaultAsync(cancellationToken);

        if (sensor is null)
        {
            return NotFound();
        }

        return Ok(sensor);
    }

    [HttpPost]
    public async Task<ActionResult<SensorResponse>> CreateSensor(
        Guid projectId,
        [FromBody] CreateSensorRequest request,
        CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == projectId, cancellationToken);

        if (!projectExists)
        {
            return NotFound();
        }

        var sensor = new Sensor
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Type = request.Type,
            Location = CreatePoint(request.Longitude, request.Latitude),
            MetadataJson = request.MetadataJson
        };

        _dbContext.Sensors.Add(sensor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new SensorResponse(
            sensor.Id,
            sensor.ProjectId,
            sensor.Type,
            sensor.Location.X,
            sensor.Location.Y,
            sensor.MetadataJson);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SensorResponse>> UpdateSensor(
        Guid projectId,
        Guid id,
        [FromBody] UpdateSensorRequest request,
        CancellationToken cancellationToken)
    {
        var sensor = await _dbContext.Sensors
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Id == id, cancellationToken);

        if (sensor is null)
        {
            return NotFound();
        }

        sensor.Type = request.Type;
        sensor.Location = CreatePoint(request.Longitude, request.Latitude);
        sensor.MetadataJson = request.MetadataJson;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new SensorResponse(
            sensor.Id,
            sensor.ProjectId,
            sensor.Type,
            sensor.Location.X,
            sensor.Location.Y,
            sensor.MetadataJson);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteSensor(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var sensor = await _dbContext.Sensors
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Id == id, cancellationToken);

        if (sensor is null)
        {
            return NotFound();
        }

        _dbContext.Sensors.Remove(sensor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("{sensorId:guid}/readings")]
    public async Task<ActionResult<IReadOnlyList<SensorReadingResponse>>> GetReadings(
        Guid projectId,
        Guid sensorId,
        CancellationToken cancellationToken)
    {
        var sensorExists = await _dbContext.Sensors
            .AsNoTracking()
            .AnyAsync(x => x.ProjectId == projectId && x.Id == sensorId, cancellationToken);

        if (!sensorExists)
        {
            return NotFound();
        }

        var readings = await _dbContext.SensorReadings
            .AsNoTracking()
            .Where(x => x.SensorId == sensorId)
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new SensorReadingResponse(
                x.Id,
                x.SensorId,
                x.Timestamp,
                x.Value))
            .ToListAsync(cancellationToken);

        return Ok(readings);
    }

    [HttpPost("{sensorId:guid}/readings")]
    public async Task<ActionResult<SensorReadingResponse>> CreateReading(
        Guid projectId,
        Guid sensorId,
        [FromBody] CreateSensorReadingRequest request,
        CancellationToken cancellationToken)
    {
        var sensorExists = await _dbContext.Sensors
            .AsNoTracking()
            .AnyAsync(x => x.ProjectId == projectId && x.Id == sensorId, cancellationToken);

        if (!sensorExists)
        {
            return NotFound();
        }

        var reading = new SensorReading
        {
            Id = Guid.NewGuid(),
            SensorId = sensorId,
            Timestamp = request.Timestamp,
            Value = request.Value
        };

        _dbContext.SensorReadings.Add(reading);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new SensorReadingResponse(
            reading.Id,
            reading.SensorId,
            reading.Timestamp,
            reading.Value);

        return Ok(response);
    }

    private static Point CreatePoint(double longitude, double latitude)
    {
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        return geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
    }
}
