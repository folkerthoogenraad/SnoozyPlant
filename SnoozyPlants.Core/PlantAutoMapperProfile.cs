using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.Core;

internal class PlantAutoMapperProfile : Profile
{
    public PlantAutoMapperProfile()
    {
        CreateMap<CreatePlantRequest, DbPlant>();
        CreateMap<UpdatePlantRequest, DbPlant>();

        CreateMap<DbPlant, Plant>();
        CreateMap<DbPlantEvent, PlantEvent>();
        CreateMap<DbPlantImage, PlantImage>();

        CreateMap<Guid, PlantId>().ConvertUsing(source => new PlantId(source));
        CreateMap<Guid, PlantEventId>().ConvertUsing(source => new PlantEventId(source));
    }
}
