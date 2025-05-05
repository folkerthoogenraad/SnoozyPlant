using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.Core;

public record PlantImage
{
    public PlantId PlantId { get; init; }

    public string Url { get; init; }


    public static readonly PlantImage Placeholder = new PlantImage() { Url = "/images/missing.png" };
}

internal record DbPlantImage
{
    [PrimaryKey]
    public Guid PlantId { get; set; }

    public string Url { get; set; }
}
