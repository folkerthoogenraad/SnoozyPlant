using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.Core;

public class PlantRequest
{
    public required string Name { get; init; }
    public string? LatinName { get; init; }
    public required string Location { get; init; }
    public int WateringIntervalInDays { get; init; }
}

public class CreatePlantRequest : PlantRequest
{
}

public class UpdatePlantRequest : PlantRequest
{
}
