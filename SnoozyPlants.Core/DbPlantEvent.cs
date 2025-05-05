using SQLite;

namespace SnoozyPlants.Core;

public record PlantEvent
{
    public PlantEventId Id { get; set; }
    public PlantId PlantId { get; set; }
    public PlantEventType Type { get; set; }
    public DateTime Timestamp { get; set; }
}

internal record DbPlantEvent
{
    [PrimaryKey]
    public Guid Id { get; set; }
    
    [Indexed]
    public Guid PlantId { get; set; }

    public PlantEventType Type { get; set; }

    public DateTime Timestamp { get; set; }
}