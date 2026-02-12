namespace GeoOps.Api.Models;

public enum ProjectStatus
{
    Draft = 0,
    Active = 1,
    OnHold = 2,
    Closed = 3
}

public enum TestStatus
{
    Pass = 0,
    Warn = 1,
    Fail = 2
}

public enum AttachmentEntityType
{
    Project = 0,
    TestResult = 1,
    Observation = 2,
    Sensor = 3
}
