namespace Backend.Api.Models;

public class TestType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public string? MetadataJson { get; set; }
}
