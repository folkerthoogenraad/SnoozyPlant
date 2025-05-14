using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.Core;

internal class DbPlantSetting
{
    [PrimaryKey]
    public string Key { get; set; }
    public string Value { get; set; }
}
