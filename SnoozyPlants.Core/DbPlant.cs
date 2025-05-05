// See https://aka.ms/new-console-template for more information

using SQLite;

namespace SnoozyPlants.Core;

public record Plant
{
    public PlantId Id { get; init; }

    public string Name { get; init; }
    
    public string? LatinName { get; init; }

    public string Location { get; init; }

    public int WateringIntervalInDays { get; init; }

    public DateTime? LastWateringDate { get; init; }

    public DateTime? NextWateringDate { get; init; }


    public static readonly Plant Placeholder = new Plant() { Id = PlantId.Empty, Name = "Lovely", Location = "At home" };
}

internal record DbPlant
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public string Name { get; set; }
    
    public string Location { get; set; }
    
    public string? LatinName { get; init; }

    public int WateringIntervalInDays { get; init; }

    [Indexed]
    public DateTime? LastWateringDate { get; set; }

    public DateTime? NextWateringDate { get; set; }
}
