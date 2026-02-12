using GeoOps.Api.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace GeoOps.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Check if seed data already exists (by specific seed user ID, not any project)
        var seedUserId = Guid.Parse("00000000-0000-0000-0000-000000000042");
        if (await db.Users.AnyAsync(u => u.Id == seedUserId))
            return; // Already seeded

        var userId = Guid.Parse("00000000-0000-0000-0000-000000000042");
        var now = DateTimeOffset.UtcNow;
        var factory = new GeometryFactory(new PrecisionModel(), 4326);

        // ─── User ───
        var user = new User
        {
            Id = userId,
            Email = "Benjamin",
            DisplayName = "Benjamin Calisch"
        };
        db.Users.Add(user);

        // ─── Test Types ───
        var icv = new TestType
        {
            Id = Guid.NewGuid(),
            Name = "Intelligent Compaction Value",
            Unit = "CMV",
            MinThreshold = 80,
            MaxThreshold = 120,
            MetadataJson = """{"description":"Compaction Meter Value from IC roller","equipment":"BOMAG BW 226"}"""
        };
        var matTemp = new TestType
        {
            Id = Guid.NewGuid(),
            Name = "Asphalt Mat Temperature",
            Unit = "°F",
            MinThreshold = 250,
            MaxThreshold = 350,
            MetadataJson = """{"description":"PMTP thermal profile measurement","method":"IR Scanner"}"""
        };
        var density = new TestType
        {
            Id = Guid.NewGuid(),
            Name = "Nuclear Density",
            Unit = "pcf",
            MinThreshold = 140,
            MaxThreshold = 155,
            MetadataJson = """{"description":"In-place density via nuclear gauge","method":"ASTM D6938"}"""
        };
        var dielectric = new TestType
        {
            Id = Guid.NewGuid(),
            Name = "Dielectric Profile",
            Unit = "DV",
            MinThreshold = 4,
            MaxThreshold = 8,
            MetadataJson = """{"description":"Dielectric value indicating moisture/void content","method":"Ground-coupled GPR"}"""
        };
        var iri = new TestType
        {
            Id = Guid.NewGuid(),
            Name = "International Roughness Index",
            Unit = "in/mi",
            MinThreshold = 0,
            MaxThreshold = 95,
            MetadataJson = """{"description":"Pavement smoothness per ProVAL analysis","method":"ASTM E1926"}"""
        };
        db.TestTypes.AddRange(icv, matTemp, density, dielectric, iri);

        // ─── Project 1: Active highway rehab ───
        var proj1 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "I-35 Pavement Rehabilitation",
            Client = "Oklahoma DOT",
            Status = ProjectStatus.Active,
            StartDate = new DateOnly(2026, 1, 15),
            EndDate = new DateOnly(2026, 8, 30)
        };

        // ─── Project 2: Active overlay ───
        var proj2 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "SH-121 Overlay & Widening",
            Client = "Texas DOT - Dallas District",
            Status = ProjectStatus.Active,
            StartDate = new DateOnly(2025, 11, 1),
            EndDate = new DateOnly(2026, 6, 15)
        };

        // ─── Project 3: Completed project ───
        var proj3 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "US-75 Bridge Approach Slab Replacement",
            Client = "City of Tulsa",
            Status = ProjectStatus.Closed,
            StartDate = new DateOnly(2025, 6, 1),
            EndDate = new DateOnly(2025, 10, 15)
        };

        // ─── Project 4: Draft / upcoming ───
        var proj4 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "I-10 Frontage Road Reconstruction",
            Client = "Texas DOT - Houston District",
            Status = ProjectStatus.Draft,
            StartDate = new DateOnly(2026, 4, 1)
        };

        db.Projects.AddRange(proj1, proj2, proj3, proj4);

        // ─── Project Memberships ───
        db.ProjectMemberships.AddRange(
            new ProjectMembership { Id = Guid.NewGuid(), ProjectId = proj1.Id, UserId = userId, Role = "Project Manager" },
            new ProjectMembership { Id = Guid.NewGuid(), ProjectId = proj2.Id, UserId = userId, Role = "QC Engineer" },
            new ProjectMembership { Id = Guid.NewGuid(), ProjectId = proj3.Id, UserId = userId, Role = "Lead Technician" },
            new ProjectMembership { Id = Guid.NewGuid(), ProjectId = proj4.Id, UserId = userId, Role = "Project Manager" }
        );

        // ─── Boundaries ───
        // I-35 corridor near Oklahoma City (roughly NE 10th St to NE 50th St)
        db.ProjectBoundaries.Add(new ProjectBoundary
        {
            Id = Guid.NewGuid(),
            ProjectId = proj1.Id,
            Polygon = factory.CreatePolygon(new[]
            {
                new Coordinate(-97.498, 35.485),
                new Coordinate(-97.492, 35.485),
                new Coordinate(-97.492, 35.520),
                new Coordinate(-97.498, 35.520),
                new Coordinate(-97.498, 35.485)
            })
        });

        // SH-121 near Plano/Frisco TX
        db.ProjectBoundaries.Add(new ProjectBoundary
        {
            Id = Guid.NewGuid(),
            ProjectId = proj2.Id,
            Polygon = factory.CreatePolygon(new[]
            {
                new Coordinate(-96.830, 33.040),
                new Coordinate(-96.790, 33.040),
                new Coordinate(-96.790, 33.075),
                new Coordinate(-96.830, 33.075),
                new Coordinate(-96.830, 33.040)
            })
        });

        // US-75 near Tulsa
        db.ProjectBoundaries.Add(new ProjectBoundary
        {
            Id = Guid.NewGuid(),
            ProjectId = proj3.Id,
            Polygon = factory.CreatePolygon(new[]
            {
                new Coordinate(-95.995, 36.145),
                new Coordinate(-95.985, 36.145),
                new Coordinate(-95.985, 36.160),
                new Coordinate(-95.995, 36.160),
                new Coordinate(-95.995, 36.145)
            })
        });

        // ─── Test Results for Project 1 (I-35 OKC) ───
        var rng = new Random(42);
        var technicians = new[] { "Benjamin Calisch", "Maria Rodriguez", "James Chen" };

        // IC compaction data along the corridor
        for (int i = 0; i < 25; i++)
        {
            var lat = 35.485 + (i * 0.0014);
            var lng = -97.495 + (rng.NextDouble() * 0.003);
            var value = 75m + (decimal)(rng.NextDouble() * 55); // 75-130 range
            var status = value >= 80 && value <= 120 ? TestStatus.Pass : (value < 80 ? TestStatus.Fail : TestStatus.Warn);

            db.TestResults.Add(new TestResult
            {
                Id = Guid.NewGuid(),
                ProjectId = proj1.Id,
                TestTypeId = icv.Id,
                Location = factory.CreatePoint(new Coordinate(lng, lat)),
                Timestamp = now.AddDays(-rng.Next(1, 30)).AddHours(rng.Next(7, 16)),
                Value = Math.Round(value, 1),
                Status = status,
                Source = "BOMAG BCM 05",
                Technician = technicians[rng.Next(technicians.Length)]
            });
        }

        // Mat temperature data
        for (int i = 0; i < 20; i++)
        {
            var lat = 35.488 + (i * 0.0015);
            var lng = -97.494 + (rng.NextDouble() * 0.002);
            var value = 240m + (decimal)(rng.NextDouble() * 130); // 240-370
            var status = value >= 250 && value <= 350 ? TestStatus.Pass : (value > 350 ? TestStatus.Fail : TestStatus.Warn);

            db.TestResults.Add(new TestResult
            {
                Id = Guid.NewGuid(),
                ProjectId = proj1.Id,
                TestTypeId = matTemp.Id,
                Location = factory.CreatePoint(new Coordinate(lng, lat)),
                Timestamp = now.AddDays(-rng.Next(1, 20)).AddHours(rng.Next(8, 15)),
                Value = Math.Round(value, 1),
                Status = status,
                Source = "Pave-IR",
                Technician = technicians[rng.Next(technicians.Length)]
            });
        }

        // Density tests (fewer — these are spot checks)
        for (int i = 0; i < 12; i++)
        {
            var lat = 35.490 + (i * 0.002);
            var lng = -97.496 + (rng.NextDouble() * 0.004);
            var value = 136m + (decimal)(rng.NextDouble() * 24); // 136-160
            var status = value >= 140 && value <= 155 ? TestStatus.Pass : (value < 140 ? TestStatus.Fail : TestStatus.Warn);

            db.TestResults.Add(new TestResult
            {
                Id = Guid.NewGuid(),
                ProjectId = proj1.Id,
                TestTypeId = density.Id,
                Location = factory.CreatePoint(new Coordinate(lng, lat)),
                Timestamp = now.AddDays(-rng.Next(1, 25)).AddHours(rng.Next(9, 14)),
                Value = Math.Round(value, 1),
                Status = status,
                Source = "Troxler 3440",
                Technician = technicians[rng.Next(technicians.Length)]
            });
        }

        // ─── Test Results for Project 2 (SH-121 Dallas) ───
        for (int i = 0; i < 18; i++)
        {
            var lat = 33.045 + (i * 0.0016);
            var lng = -96.815 + (rng.NextDouble() * 0.02);
            var value = 78m + (decimal)(rng.NextDouble() * 50);
            var status = value >= 80 && value <= 120 ? TestStatus.Pass : (value < 80 ? TestStatus.Fail : TestStatus.Warn);

            db.TestResults.Add(new TestResult
            {
                Id = Guid.NewGuid(),
                ProjectId = proj2.Id,
                TestTypeId = icv.Id,
                Location = factory.CreatePoint(new Coordinate(lng, lat)),
                Timestamp = now.AddDays(-rng.Next(1, 45)).AddHours(rng.Next(7, 16)),
                Value = Math.Round(value, 1),
                Status = status,
                Source = "CAT CS56B",
                Technician = technicians[rng.Next(technicians.Length)]
            });
        }

        // IRI data for completed project 3
        for (int i = 0; i < 10; i++)
        {
            var lat = 36.148 + (i * 0.001);
            var lng = -95.990 + (rng.NextDouble() * 0.004);
            var value = 30m + (decimal)(rng.NextDouble() * 80); // 30-110
            var status = value <= 95 ? TestStatus.Pass : TestStatus.Fail;

            db.TestResults.Add(new TestResult
            {
                Id = Guid.NewGuid(),
                ProjectId = proj3.Id,
                TestTypeId = iri.Id,
                Location = factory.CreatePoint(new Coordinate(lng, lat)),
                Timestamp = now.AddDays(-120 + i * 5),
                Value = Math.Round(value, 1),
                Status = status,
                Source = "ProVAL",
                Technician = "Benjamin Calisch"
            });
        }

        // ─── Observations ───
        db.Observations.AddRange(
            new Observation
            {
                Id = Guid.NewGuid(), ProjectId = proj1.Id,
                Location = factory.CreatePoint(new Coordinate(-97.495, 35.498)),
                Timestamp = now.AddDays(-5),
                Note = "Segregation observed in mat — notified paving crew to adjust auger speed and material feed rate.",
                Tags = "segregation,mat-quality"
            },
            new Observation
            {
                Id = Guid.NewGuid(), ProjectId = proj1.Id,
                Location = factory.CreatePoint(new Coordinate(-97.493, 35.505)),
                Timestamp = now.AddDays(-3),
                Note = "Roller pattern deviation in NB lane. Operator corrected after IC feedback showed low CMV in wheel path.",
                Tags = "compaction,roller-pattern"
            },
            new Observation
            {
                Id = Guid.NewGuid(), ProjectId = proj1.Id,
                Location = factory.CreatePoint(new Coordinate(-97.496, 35.512)),
                Timestamp = now.AddDays(-1),
                Note = "Rain delay — covered fresh mat with tarps. Resumed paving at 1400 after surface moisture check.",
                Tags = "weather,delay"
            },
            new Observation
            {
                Id = Guid.NewGuid(), ProjectId = proj2.Id,
                Location = factory.CreatePoint(new Coordinate(-96.810, 33.055)),
                Timestamp = now.AddDays(-10),
                Note = "Thermal profile shows cold spot at longitudinal joint. Adjusted screed crown to improve uniformity.",
                Tags = "thermal,joint-quality"
            },
            new Observation
            {
                Id = Guid.NewGuid(), ProjectId = proj2.Id,
                Location = factory.CreatePoint(new Coordinate(-96.800, 33.062)),
                Timestamp = now.AddDays(-7),
                Note = "Dielectric reading elevated near drainage structure — possible moisture intrusion from subgrade.",
                Tags = "dielectric,moisture,subgrade"
            },
            new Observation
            {
                Id = Guid.NewGuid(), ProjectId = proj3.Id,
                Location = factory.CreatePoint(new Coordinate(-95.990, 36.152)),
                Timestamp = now.AddDays(-100),
                Note = "Final IRI acceptance — all 10 profile sections meet ODOT smoothness spec. Project approved for closeout.",
                Tags = "acceptance,smoothness,final"
            }
        );

        // ─── Sensors for Project 1 ───
        var sensor1 = new Sensor
        {
            Id = Guid.NewGuid(),
            ProjectId = proj1.Id,
            Type = "Embedded Temperature Probe",
            Location = factory.CreatePoint(new Coordinate(-97.495, 35.500)),
            MetadataJson = """{"depth":"2 inches","model":"Sensirion STS40"}"""
        };
        var sensor2 = new Sensor
        {
            Id = Guid.NewGuid(),
            ProjectId = proj1.Id,
            Type = "Ambient Weather Station",
            Location = factory.CreatePoint(new Coordinate(-97.497, 35.495)),
            MetadataJson = """{"model":"Davis Vantage Pro2","measures":"temp,humidity,wind,precip"}"""
        };
        var sensor3 = new Sensor
        {
            Id = Guid.NewGuid(),
            ProjectId = proj2.Id,
            Type = "Strain Gauge",
            Location = factory.CreatePoint(new Coordinate(-96.810, 33.050)),
            MetadataJson = """{"depth":"4 inches","model":"Geokon 3900","purpose":"structural monitoring"}"""
        };
        db.Sensors.AddRange(sensor1, sensor2, sensor3);

        // ─── Sensor Readings (temperature probe - hourly for 7 days) ───
        for (int day = 0; day < 7; day++)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var ts = now.AddDays(-7 + day).Date.AddHours(hour);
                // Simulate daily temperature cycle: cool at night, warm midday
                var baseTemp = 45m + (decimal)(15 * Math.Sin((hour - 6) * Math.PI / 12));
                var temp = baseTemp + (decimal)(rng.NextDouble() * 4 - 2);

                db.SensorReadings.Add(new SensorReading
                {
                    Id = Guid.NewGuid(),
                    SensorId = sensor1.Id,
                    Timestamp = new DateTimeOffset(ts, TimeSpan.Zero),
                    Value = Math.Round(temp, 1)
                });
            }
        }

        // Weather station readings (less frequent)
        for (int day = 0; day < 7; day++)
        {
            for (int hour = 6; hour <= 18; hour += 2)
            {
                var ts = now.AddDays(-7 + day).Date.AddHours(hour);
                var windSpeed = 5m + (decimal)(rng.NextDouble() * 15);
                db.SensorReadings.Add(new SensorReading
                {
                    Id = Guid.NewGuid(),
                    SensorId = sensor2.Id,
                    Timestamp = new DateTimeOffset(ts, TimeSpan.Zero),
                    Value = Math.Round(windSpeed, 1)
                });
            }
        }

        // ─── Audit Log entries ───
        db.AuditLogs.AddRange(
            new AuditLog { Id = Guid.NewGuid(), Actor = "Benjamin Calisch", Action = "Created", EntityType = "Project", EntityId = proj1.Id, Timestamp = now.AddDays(-45) },
            new AuditLog { Id = Guid.NewGuid(), Actor = "Benjamin Calisch", Action = "Created", EntityType = "Project", EntityId = proj2.Id, Timestamp = now.AddDays(-90) },
            new AuditLog { Id = Guid.NewGuid(), Actor = "Benjamin Calisch", Action = "StatusChanged", EntityType = "Project", EntityId = proj3.Id, Timestamp = now.AddDays(-60), MetadataJson = """{"from":"Active","to":"Closed"}""" },
            new AuditLog { Id = Guid.NewGuid(), Actor = "Benjamin Calisch", Action = "Created", EntityType = "Project", EntityId = proj4.Id, Timestamp = now.AddDays(-10) },
            new AuditLog { Id = Guid.NewGuid(), Actor = "System", Action = "BatchIngested", EntityType = "TestResult", EntityId = proj1.Id, Timestamp = now.AddDays(-5), MetadataJson = """{"count":25,"source":"BOMAG BCM 05"}""" }
        );

        await db.SaveChangesAsync();
    }
}
