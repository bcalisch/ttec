namespace Ticketing.Api.Models;

public enum TicketStatus
{
    Open,
    InProgress,
    AwaitingCustomer,
    AwaitingParts,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TicketCategory
{
    Software,
    Hardware,
    Calibration,
    Training,
    FieldSupport,
    Other
}

public enum EquipmentType
{
    Roller,
    Paver,
    MillingMachine,
    Sensor,
    Software,
    Other
}

public enum EquipmentManufacturer
{
    BOMAG,
    CAT,
    HAMM,
    Volvo,
    Dynapac,
    Other
}
