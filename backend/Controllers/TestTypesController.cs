using Backend.Api.Contracts.TestTypes;
using Backend.Api.Data;
using Backend.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/test-types")]
public class TestTypesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public TestTypesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TestType>>> GetTestTypes(CancellationToken cancellationToken)
    {
        var testTypes = await _dbContext.TestTypes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(testTypes);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TestType>> GetTestType(Guid id, CancellationToken cancellationToken)
    {
        var testType = await _dbContext.TestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (testType is null)
        {
            return NotFound();
        }

        return Ok(testType);
    }

    [HttpPost]
    public async Task<ActionResult<TestType>> CreateTestType([FromBody] CreateTestTypeRequest request, CancellationToken cancellationToken)
    {
        var testType = new TestType
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Unit = request.Unit,
            MinThreshold = request.MinThreshold,
            MaxThreshold = request.MaxThreshold,
            MetadataJson = request.MetadataJson
        };

        _dbContext.TestTypes.Add(testType);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetTestType), new { id = testType.Id }, testType);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TestType>> UpdateTestType(Guid id, [FromBody] UpdateTestTypeRequest request, CancellationToken cancellationToken)
    {
        var testType = await _dbContext.TestTypes
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (testType is null)
        {
            return NotFound();
        }

        testType.Name = request.Name;
        testType.Unit = request.Unit;
        testType.MinThreshold = request.MinThreshold;
        testType.MaxThreshold = request.MaxThreshold;
        testType.MetadataJson = request.MetadataJson;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(testType);
    }
}
