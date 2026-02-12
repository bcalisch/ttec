using GeoOps.Api.Data;
using GeoOps.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace GeoOps.Api.Controllers;

[ApiController]
[Route("api/seed")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public SeedController(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [HttpPost]
    public async Task<ActionResult> Seed(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        // Clear existing data in FK-safe order
        _dbContext.SensorReadings.RemoveRange(await _dbContext.SensorReadings.ToListAsync(cancellationToken));
        _dbContext.Sensors.RemoveRange(await _dbContext.Sensors.ToListAsync(cancellationToken));
        _dbContext.Observations.RemoveRange(await _dbContext.Observations.ToListAsync(cancellationToken));
        _dbContext.TestResults.RemoveRange(await _dbContext.TestResults.ToListAsync(cancellationToken));
        _dbContext.TestTypes.RemoveRange(await _dbContext.TestTypes.ToListAsync(cancellationToken));
        _dbContext.ProjectBoundaries.RemoveRange(await _dbContext.ProjectBoundaries.ToListAsync(cancellationToken));
        _dbContext.Attachments.RemoveRange(await _dbContext.Attachments.ToListAsync(cancellationToken));
        _dbContext.AuditLogs.RemoveRange(await _dbContext.AuditLogs.ToListAsync(cancellationToken));
        _dbContext.IngestBatches.RemoveRange(await _dbContext.IngestBatches.ToListAsync(cancellationToken));
        _dbContext.ProjectMemberships.RemoveRange(await _dbContext.ProjectMemberships.ToListAsync(cancellationToken));
        _dbContext.Projects.RemoveRange(await _dbContext.Projects.ToListAsync(cancellationToken));
        _dbContext.Users.RemoveRange(await _dbContext.Users.ToListAsync(cancellationToken));
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Projects
        var project1 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Highway 101 Expansion",
            Client = "Colorado DOT",
            Status = ProjectStatus.Active,
            StartDate = new DateOnly(2025, 3, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };

        var project2 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Downtown Bridge Replacement",
            Client = "City of Denver",
            Status = ProjectStatus.Active,
            StartDate = new DateOnly(2025, 6, 15),
            EndDate = new DateOnly(2027, 6, 15)
        };

        _dbContext.Projects.AddRange(project1, project2);

        // Project boundaries (Denver area)
        var boundary1 = new ProjectBoundary
        {
            Id = Guid.NewGuid(),
            ProjectId = project1.Id,
            Polygon = geometryFactory.CreatePolygon(new[]
            {
                new Coordinate(-104.95, 39.72),
                new Coordinate(-104.90, 39.72),
                new Coordinate(-104.90, 39.76),
                new Coordinate(-104.95, 39.76),
                new Coordinate(-104.95, 39.72)
            })
        };

        var boundary2 = new ProjectBoundary
        {
            Id = Guid.NewGuid(),
            ProjectId = project2.Id,
            Polygon = geometryFactory.CreatePolygon(new[]
            {
                new Coordinate(-104.99, 39.74),
                new Coordinate(-104.96, 39.74),
                new Coordinate(-104.96, 39.77),
                new Coordinate(-104.99, 39.77),
                new Coordinate(-104.99, 39.74)
            })
        };

        _dbContext.ProjectBoundaries.AddRange(boundary1, boundary2);

        // Test types
        var density = new TestType
        {
            Id = Guid.NewGuid(),
            Name = "Density",
            Unit = "pcf",
            MinThreshold = 95m,
            MaxThreshold = 105m
        };

        var moisture = new TestType
        {
            Id = Guid.NewGuid(),
            Name = "Moisture Content",
            Unit = "%",
            MinThreshold = 8m,
            MaxThreshold = 15m
        };

        var concrete = new TestType
        {
            Id = Guid.NewGuid(),
            Name = "Concrete Strength",
            Unit = "psi",
            MinThreshold = 3000m,
            MaxThreshold = 5000m
        };

        _dbContext.TestTypes.AddRange(density, moisture, concrete);

        // Test results (30 spread across both projects)
        var testTypes = new[] { density, moisture, concrete };
        var statuses = new[] { TestStatus.Pass, TestStatus.Pass, TestStatus.Pass, TestStatus.Warn, TestStatus.Fail };
        var random = new Random(42);
        var testResults = new List<TestResult>();

        for (int i = 0; i < 15; i++)
        {
            var tt = testTypes[i % 3];
            var st = statuses[i % 5];
            var value = tt.Name switch
            {
                "Density" => st == TestStatus.Pass ? 98m + (decimal)(random.NextDouble() * 5) :
                             st == TestStatus.Warn ? 94m + (decimal)(random.NextDouble() * 2) :
                             90m + (decimal)(random.NextDouble() * 3),
                "Moisture Content" => st == TestStatus.Pass ? 9m + (decimal)(random.NextDouble() * 5) :
                                     st == TestStatus.Warn ? 7m + (decimal)(random.NextDouble() * 2) :
                                     16m + (decimal)(random.NextDouble() * 3),
                _ => st == TestStatus.Pass ? 3500m + (decimal)(random.NextDouble() * 1000) :
                     st == TestStatus.Warn ? 2800m + (decimal)(random.NextDouble() * 400) :
                     2000m + (decimal)(random.NextDouble() * 500)
            };

            testResults.Add(new TestResult
            {
                Id = Guid.NewGuid(),
                ProjectId = project1.Id,
                TestTypeId = tt.Id,
                Location = geometryFactory.CreatePoint(new Coordinate(
                    -104.95 + random.NextDouble() * 0.05,
                    39.72 + random.NextDouble() * 0.04)),
                Timestamp = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 90)),
                Value = Math.Round(value, 2),
                Status = st,
                Source = "Field Test",
                Technician = $"Tech {(i % 3) + 1}"
            });
        }

        for (int i = 0; i < 15; i++)
        {
            var tt = testTypes[i % 3];
            var st = statuses[(i + 2) % 5];
            var value = tt.Name switch
            {
                "Density" => st == TestStatus.Pass ? 99m + (decimal)(random.NextDouble() * 4) :
                             st == TestStatus.Warn ? 93m + (decimal)(random.NextDouble() * 3) :
                             88m + (decimal)(random.NextDouble() * 4),
                "Moisture Content" => st == TestStatus.Pass ? 10m + (decimal)(random.NextDouble() * 4) :
                                     st == TestStatus.Warn ? 6m + (decimal)(random.NextDouble() * 3) :
                                     17m + (decimal)(random.NextDouble() * 4),
                _ => st == TestStatus.Pass ? 3600m + (decimal)(random.NextDouble() * 800) :
                     st == TestStatus.Warn ? 2700m + (decimal)(random.NextDouble() * 500) :
                     1800m + (decimal)(random.NextDouble() * 600)
            };

            testResults.Add(new TestResult
            {
                Id = Guid.NewGuid(),
                ProjectId = project2.Id,
                TestTypeId = tt.Id,
                Location = geometryFactory.CreatePoint(new Coordinate(
                    -104.99 + random.NextDouble() * 0.03,
                    39.74 + random.NextDouble() * 0.03)),
                Timestamp = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 60)),
                Value = Math.Round(value, 2),
                Status = st,
                Source = "Lab Test",
                Technician = $"Tech {(i % 4) + 1}"
            });
        }

        _dbContext.TestResults.AddRange(testResults);

        // Observations
        var observations = new List<Observation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = project1.Id,
                Location = geometryFactory.CreatePoint(new Coordinate(-104.93, 39.73)),
                Timestamp = DateTimeOffset.UtcNow.AddDays(-5),
                Note = "Visible cracking along the east shoulder. Needs immediate attention.",
                Tags = "cracking,shoulder,urgent"
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = project1.Id,
                Location = geometryFactory.CreatePoint(new Coordinate(-104.92, 39.74)),
                Timestamp = DateTimeOffset.UtcNow.AddDays(-12),
                Note = "Subgrade moisture levels appear elevated after recent rainfall.",
                Tags = "moisture,subgrade,weather"
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = project1.Id,
                Location = geometryFactory.CreatePoint(new Coordinate(-104.91, 39.75)),
                Timestamp = DateTimeOffset.UtcNow.AddDays(-20),
                Note = "Compaction equipment calibration verified on site.",
                Tags = "equipment,calibration"
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = project2.Id,
                Location = geometryFactory.CreatePoint(new Coordinate(-104.98, 39.75)),
                Timestamp = DateTimeOffset.UtcNow.AddDays(-3),
                Note = "Bridge deck pour completed successfully. Curing process started.",
                Tags = "concrete,bridge,pour"
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = project2.Id,
                Location = geometryFactory.CreatePoint(new Coordinate(-104.97, 39.76)),
                Timestamp = DateTimeOffset.UtcNow.AddDays(-8),
                Note = "Rebar placement inspected and approved for section B2.",
                Tags = "rebar,inspection,approved"
            }
        };

        _dbContext.Observations.AddRange(observations);

        // Sensors
        var sensor1 = new Sensor
        {
            Id = Guid.NewGuid(),
            ProjectId = project1.Id,
            Type = "Strain Gauge",
            Location = geometryFactory.CreatePoint(new Coordinate(-104.93, 39.74)),
            MetadataJson = "{\"manufacturer\":\"Geokon\",\"model\":\"4200\"}"
        };

        var sensor2 = new Sensor
        {
            Id = Guid.NewGuid(),
            ProjectId = project2.Id,
            Type = "Temperature Sensor",
            Location = geometryFactory.CreatePoint(new Coordinate(-104.975, 39.755)),
            MetadataJson = "{\"manufacturer\":\"Campbell Scientific\",\"model\":\"109\"}"
        };

        _dbContext.Sensors.AddRange(sensor1, sensor2);

        // Sensor readings
        var sensorReadings = new List<SensorReading>();
        for (int i = 0; i < 4; i++)
        {
            sensorReadings.Add(new SensorReading
            {
                Id = Guid.NewGuid(),
                SensorId = sensor1.Id,
                Timestamp = DateTimeOffset.UtcNow.AddHours(-i * 6),
                Value = 250m + (decimal)(random.NextDouble() * 50)
            });
        }

        for (int i = 0; i < 3; i++)
        {
            sensorReadings.Add(new SensorReading
            {
                Id = Guid.NewGuid(),
                SensorId = sensor2.Id,
                Timestamp = DateTimeOffset.UtcNow.AddHours(-i * 8),
                Value = 18m + (decimal)(random.NextDouble() * 12)
            });
        }

        _dbContext.SensorReadings.AddRange(sensorReadings);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Seed data created successfully.",
            Projects = 2,
            Boundaries = 2,
            TestTypes = 3,
            TestResults = testResults.Count,
            Observations = observations.Count,
            Sensors = 2,
            SensorReadings = sensorReadings.Count
        });
    }
}
