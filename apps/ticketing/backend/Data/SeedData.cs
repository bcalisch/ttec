using Ticketing.Api.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Ticketing.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var seedUserId = Guid.Parse("00000000-0000-0000-0000-000000000042");
        if (await db.Users.AnyAsync(u => u.Id == seedUserId))
            return;

        var now = DateTimeOffset.UtcNow;
        var factory = new GeometryFactory(new PrecisionModel(), 4326);

        // ─── User ───
        db.Users.Add(new User
        {
            Id = seedUserId,
            Email = "Benjamin",
            DisplayName = "Benjamin Calisch"
        });

        // ─── Equipment ───
        var eq1 = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "BOMAG BW 226",
            SerialNumber = "BW226-2024-001",
            Type = EquipmentType.Roller,
            Manufacturer = EquipmentManufacturer.BOMAG,
            Model = "BW 226 BVC-5",
            CreatedAt = now.AddDays(-90)
        };
        var eq2 = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "CAT CS56B",
            SerialNumber = "CS56B-2024-042",
            Type = EquipmentType.Roller,
            Manufacturer = EquipmentManufacturer.CAT,
            Model = "CS56B",
            CreatedAt = now.AddDays(-60)
        };
        var eq3 = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "HAMM HD+ 120i",
            SerialNumber = "HD120I-2023-118",
            Type = EquipmentType.Roller,
            Manufacturer = EquipmentManufacturer.HAMM,
            Model = "HD+ 120i VV-HF",
            CreatedAt = now.AddDays(-120)
        };
        var eq4 = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Pave-IR Thermal Scanner",
            SerialNumber = "PIR-2024-007",
            Type = EquipmentType.Sensor,
            Manufacturer = EquipmentManufacturer.Other,
            Model = "Pave-IR",
            CreatedAt = now.AddDays(-45)
        };
        var eq5 = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Troxler 3440 Nuclear Gauge",
            SerialNumber = "TRX3440-2022-091",
            Type = EquipmentType.Sensor,
            Manufacturer = EquipmentManufacturer.Other,
            Model = "3440",
            CreatedAt = now.AddDays(-200)
        };
        db.Equipment.AddRange(eq1, eq2, eq3, eq4, eq5);

        // ─── Tickets ───
        var ticket1 = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "BOMAG BW 226 CMV sensor returning zero values",
            Description = "IC roller sensor on BW 226 (#BW226-2024-001) consistently returning CMV=0 during compaction. Suspected wiring issue at sensor junction box. Affects data quality on I-35 project.",
            Status = TicketStatus.InProgress,
            Priority = TicketPriority.Critical,
            Category = TicketCategory.Hardware,
            ReportedBy = "Benjamin Calisch",
            AssignedTo = "Maria Rodriguez",
            Location = factory.CreatePoint(new Coordinate(-97.495, 35.500)),
            EquipmentId = eq1.Id,
            SourceApp = "geoops",
            SourceEntityType = "project",
            SourceEntityId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-1),
            SlaDeadline = now.AddDays(-3).AddHours(4)
        };

        var ticket2 = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "Pave-IR thermal scanner calibration drift",
            Description = "Temperature readings drifting +15°F compared to reference thermometer. Needs factory recalibration.",
            Status = TicketStatus.AwaitingParts,
            Priority = TicketPriority.High,
            Category = TicketCategory.Calibration,
            ReportedBy = "James Chen",
            AssignedTo = "Benjamin Calisch",
            EquipmentId = eq4.Id,
            CreatedAt = now.AddDays(-7),
            UpdatedAt = now.AddDays(-2),
            SlaDeadline = now.AddDays(-7).AddHours(8)
        };

        var ticket3 = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "GeoOps map layer not loading test results",
            Description = "Test results for SH-121 project not displaying on the map. API returns data but frontend rendering fails.",
            Status = TicketStatus.Open,
            Priority = TicketPriority.Medium,
            Category = TicketCategory.Software,
            ReportedBy = "Benjamin Calisch",
            SourceApp = "geoops",
            SourceEntityType = "project",
            SourceEntityId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now.AddDays(-1),
            SlaDeadline = now.AddDays(-1).AddHours(24)
        };

        var ticket4 = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "New technician IC roller training request",
            Description = "New field technician needs training on BOMAG IC roller operation and BCM 05 data collection software.",
            Status = TicketStatus.Open,
            Priority = TicketPriority.Low,
            Category = TicketCategory.Training,
            ReportedBy = "Maria Rodriguez",
            CreatedAt = now.AddDays(-5),
            UpdatedAt = now.AddDays(-5),
            SlaDeadline = now.AddDays(-5).AddHours(72)
        };

        var ticket5 = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "Nuclear gauge annual calibration due",
            Description = "Troxler 3440 (#TRX3440-2022-091) annual calibration certificate expires in 2 weeks. Schedule factory calibration.",
            Status = TicketStatus.Resolved,
            Priority = TicketPriority.Medium,
            Category = TicketCategory.Calibration,
            ReportedBy = "Benjamin Calisch",
            AssignedTo = "James Chen",
            EquipmentId = eq5.Id,
            CreatedAt = now.AddDays(-20),
            UpdatedAt = now.AddDays(-5),
            ResolvedAt = now.AddDays(-5),
            SlaDeadline = now.AddDays(-20).AddHours(24)
        };

        var ticket6 = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "CAT CS56B hydraulic pressure warning",
            Description = "Low hydraulic pressure warning during operation. Roller still functional but drum vibration inconsistent.",
            Status = TicketStatus.Closed,
            Priority = TicketPriority.High,
            Category = TicketCategory.Hardware,
            ReportedBy = "James Chen",
            AssignedTo = "Maria Rodriguez",
            Location = factory.CreatePoint(new Coordinate(-96.810, 33.055)),
            EquipmentId = eq2.Id,
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now.AddDays(-20),
            ResolvedAt = now.AddDays(-22),
            SlaDeadline = now.AddDays(-30).AddHours(8)
        };

        var ticket7 = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "Field support: GPS base station setup at I-10 site",
            Description = "New project at I-10 Frontage Road needs GPS base station deployment for RTK corrections.",
            Status = TicketStatus.AwaitingCustomer,
            Priority = TicketPriority.Medium,
            Category = TicketCategory.FieldSupport,
            ReportedBy = "Benjamin Calisch",
            Location = factory.CreatePoint(new Coordinate(-95.370, 29.760)),
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-1),
            SlaDeadline = now.AddDays(-2).AddHours(24)
        };

        db.Tickets.AddRange(ticket1, ticket2, ticket3, ticket4, ticket5, ticket6, ticket7);

        // ─── Comments ───
        db.TicketComments.AddRange(
            new TicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = ticket1.Id,
                Author = "Maria Rodriguez",
                Body = "Inspected junction box on-site. Found corroded connector at pin 3. Ordering replacement harness.",
                IsInternal = false,
                CreatedAt = now.AddDays(-2)
            },
            new TicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = ticket1.Id,
                Author = "Benjamin Calisch",
                Body = "Can we get an ETA on the replacement part? I-35 paving resumes Monday.",
                IsInternal = false,
                CreatedAt = now.AddDays(-1).AddHours(3)
            },
            new TicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = ticket1.Id,
                Author = "Maria Rodriguez",
                Body = "BOMAG parts warehouse shows 2-day shipping. Should arrive Friday.",
                IsInternal = true,
                CreatedAt = now.AddDays(-1).AddHours(5)
            },
            new TicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = ticket2.Id,
                Author = "James Chen",
                Body = "Contacted manufacturer. Sending RMA form for factory recalibration.",
                IsInternal = false,
                CreatedAt = now.AddDays(-5)
            },
            new TicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = ticket5.Id,
                Author = "James Chen",
                Body = "Calibration complete. New certificate valid through 2027-02-01.",
                IsInternal = false,
                CreatedAt = now.AddDays(-5)
            }
        );

        // ─── Time Entries ───
        db.TimeEntries.AddRange(
            new TimeEntry
            {
                Id = Guid.NewGuid(),
                TicketId = ticket1.Id,
                Technician = "Maria Rodriguez",
                Hours = 2.5m,
                HourlyRate = 250m,
                Description = "On-site diagnosis of CMV sensor wiring",
                CreatedAt = now.AddDays(-2)
            },
            new TimeEntry
            {
                Id = Guid.NewGuid(),
                TicketId = ticket1.Id,
                Technician = "Maria Rodriguez",
                Hours = 1.0m,
                HourlyRate = 250m,
                Description = "Parts sourcing and order placement",
                CreatedAt = now.AddDays(-1)
            },
            new TimeEntry
            {
                Id = Guid.NewGuid(),
                TicketId = ticket2.Id,
                Technician = "James Chen",
                Hours = 1.5m,
                HourlyRate = 250m,
                Description = "Comparison testing against reference thermometer",
                CreatedAt = now.AddDays(-6)
            },
            new TimeEntry
            {
                Id = Guid.NewGuid(),
                TicketId = ticket5.Id,
                Technician = "James Chen",
                Hours = 4.0m,
                HourlyRate = 250m,
                Description = "Gauge transport and factory calibration appointment",
                CreatedAt = now.AddDays(-10)
            },
            new TimeEntry
            {
                Id = Guid.NewGuid(),
                TicketId = ticket6.Id,
                Technician = "Maria Rodriguez",
                Hours = 3.0m,
                HourlyRate = 250m,
                Description = "Hydraulic system diagnosis and repair",
                CreatedAt = now.AddDays(-25)
            },
            new TimeEntry
            {
                Id = Guid.NewGuid(),
                TicketId = ticket6.Id,
                Technician = "Maria Rodriguez",
                Hours = 1.5m,
                HourlyRate = 250m,
                Description = "Post-repair verification and testing",
                CreatedAt = now.AddDays(-22)
            }
        );

        // ─── Knowledge Articles ───
        db.KnowledgeArticles.AddRange(
            new KnowledgeArticle
            {
                Id = Guid.NewGuid(),
                Title = "BOMAG IC Roller CMV Sensor Troubleshooting Guide",
                Content = "Common issues with BOMAG BW 226 CMV sensors:\n\n1. **Zero readings**: Check junction box connectors for corrosion. Pin 3 (signal) and Pin 5 (ground) are most susceptible.\n2. **Intermittent readings**: Verify cable routing away from hydraulic lines. Vibration can cause intermittent connections.\n3. **Offset readings**: Run calibration procedure per BOMAG BCM 05 manual section 4.3.\n\nAlways verify against a known-good reference measurement before condemning the sensor.",
                Tags = "bomag,ic-roller,cmv,sensor,troubleshooting",
                IsPublished = true,
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now.AddDays(-10)
            },
            new KnowledgeArticle
            {
                Id = Guid.NewGuid(),
                Title = "Thermal Scanner Calibration Procedure",
                Content = "Annual calibration procedure for Pave-IR and similar PMTP thermal scanners:\n\n1. Set up blackbody reference source at 250°F, 300°F, and 350°F.\n2. Record scanner readings at each setpoint.\n3. If deviation exceeds ±5°F at any point, send to manufacturer for adjustment.\n4. Document calibration results in equipment log.\n\nNote: Field calibration checks should be performed weekly using a NIST-traceable reference thermometer.",
                Tags = "thermal,calibration,pave-ir,pmtp",
                IsPublished = true,
                CreatedAt = now.AddDays(-45),
                UpdatedAt = now.AddDays(-45)
            },
            new KnowledgeArticle
            {
                Id = Guid.NewGuid(),
                Title = "Nuclear Gauge Safety and Handling Procedures",
                Content = "Safety requirements for Troxler 3440 and similar nuclear density gauges:\n\n1. Only licensed operators may transport or operate the gauge.\n2. Maintain minimum 6-foot distance when not in use.\n3. Store in approved, locked transport container.\n4. Wear dosimeter badge at all times during operation.\n5. Report any suspected damage or exposure immediately.\n\nCalibration must be current — expired calibration means the gauge cannot be used on any project.",
                Tags = "nuclear-gauge,safety,troxler,density",
                IsPublished = true,
                CreatedAt = now.AddDays(-90),
                UpdatedAt = now.AddDays(-30)
            },
            new KnowledgeArticle
            {
                Id = Guid.NewGuid(),
                Title = "GeoOps Integration: Creating Tickets from Projects",
                Content = "The ticketing system integrates with GeoOps via deep-linking:\n\n1. From a GeoOps project dashboard, click 'Create Support Ticket'.\n2. The ticketing app opens with pre-filled source reference (project ID).\n3. Tickets linked to GeoOps projects show a 'View in GeoOps' link.\n\nThis is a loose coupling — no data is shared between databases. The link is purely navigational.",
                Tags = "geoops,integration,workflow",
                IsPublished = true,
                CreatedAt = now.AddDays(-15),
                UpdatedAt = now.AddDays(-15)
            },
            new KnowledgeArticle
            {
                Id = Guid.NewGuid(),
                Title = "Draft: SLA Policy for Field Equipment Issues",
                Content = "Proposed SLA windows:\n- Critical (equipment down, project blocked): 4 hours\n- High (degraded performance): 8 hours\n- Medium (workaround available): 24 hours\n- Low (informational/training): 72 hours\n\nThis is a DRAFT — not yet approved by management.",
                Tags = "sla,policy,draft",
                IsPublished = false,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5)
            }
        );

        await db.SaveChangesAsync();
    }
}
