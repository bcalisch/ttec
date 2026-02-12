using Ticketing.Api.Contracts.Equipment;
using Ticketing.Api.Data;
using Ticketing.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ticketing.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/equipment")]
public class EquipmentController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public EquipmentController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EquipmentResponse>>> GetEquipment(CancellationToken cancellationToken)
    {
        var equipment = await _dbContext.Equipment
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new EquipmentResponse(x.Id, x.Name, x.SerialNumber, x.Type, x.Manufacturer, x.Model, x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(equipment);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EquipmentResponse>> GetEquipmentById(Guid id, CancellationToken cancellationToken)
    {
        var equipment = await _dbContext.Equipment
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (equipment is null)
            return NotFound();

        return Ok(new EquipmentResponse(equipment.Id, equipment.Name, equipment.SerialNumber, equipment.Type, equipment.Manufacturer, equipment.Model, equipment.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<EquipmentResponse>> CreateEquipment(
        [FromBody] CreateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var duplicateSerial = await _dbContext.Equipment
            .AsNoTracking()
            .AnyAsync(x => x.SerialNumber == request.SerialNumber, cancellationToken);

        if (duplicateSerial)
            return Conflict(new { message = "Equipment with this serial number already exists." });

        var equipment = new Models.Equipment
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            SerialNumber = request.SerialNumber,
            Type = request.Type,
            Manufacturer = request.Manufacturer,
            Model = request.Model,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Equipment.Add(equipment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new EquipmentResponse(equipment.Id, equipment.Name, equipment.SerialNumber, equipment.Type, equipment.Manufacturer, equipment.Model, equipment.CreatedAt);
        return CreatedAtAction(nameof(GetEquipmentById), new { id = equipment.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EquipmentResponse>> UpdateEquipment(
        Guid id,
        [FromBody] UpdateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var equipment = await _dbContext.Equipment
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (equipment is null)
            return NotFound();

        var duplicateSerial = await _dbContext.Equipment
            .AsNoTracking()
            .AnyAsync(x => x.SerialNumber == request.SerialNumber && x.Id != id, cancellationToken);

        if (duplicateSerial)
            return Conflict(new { message = "Equipment with this serial number already exists." });

        equipment.Name = request.Name;
        equipment.SerialNumber = request.SerialNumber;
        equipment.Type = request.Type;
        equipment.Manufacturer = request.Manufacturer;
        equipment.Model = request.Model;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new EquipmentResponse(equipment.Id, equipment.Name, equipment.SerialNumber, equipment.Type, equipment.Manufacturer, equipment.Model, equipment.CreatedAt));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteEquipment(Guid id, CancellationToken cancellationToken)
    {
        var equipment = await _dbContext.Equipment
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (equipment is null)
            return NotFound();

        // Unlink tickets referencing this equipment
        var linkedTickets = await _dbContext.Tickets
            .Where(x => x.EquipmentId == id)
            .ToListAsync(cancellationToken);
        foreach (var ticket in linkedTickets)
            ticket.EquipmentId = null;

        _dbContext.Equipment.Remove(equipment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
