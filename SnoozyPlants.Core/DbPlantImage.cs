using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.Core;

public record PlantImage
{
    public string MimeType { get; set; }
    public byte[]? Data { get; set; }
}

internal record DbPlantImage
{
    [PrimaryKey]
    public Guid PlantId { get; set; }

    public Guid Version { get; set; }

    public string MimeType { get; set; }

    public byte[]? Data { get; set; }
}
