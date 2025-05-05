using AutoMapper;
using SQLite;
using System.Diagnostics;

namespace SnoozyPlants.Core;

public class PlantRepository 
{
    private SQLiteConnection _connection;
    private IMapper _mapper;

    private Dictionary<PlantId, DbPlantImage> _images = new();

    public PlantRepository(PlantDatabaseConfiguration config)
    {
        bool seed = !File.Exists(config.FilePath);

        var options = new SQLiteConnectionString(config.FilePath, true);
        _connection = new SQLiteConnection(options);

        _connection.CreateTable<DbPlant>();
        _connection.CreateTable<DbPlantEvent>();
        _connection.CreateTable<DbPlantImage>();

        var configuration = new MapperConfiguration(cfg => {
            cfg.AddProfile<PlantAutoMapperProfile>();
        });

        _mapper = configuration.CreateMapper();

        if (seed)
        {
            SeedDatabase();
        }
    }

    public PlantId CreatePlant(CreatePlantRequest request)
    {
        var plant = new DbPlant()
        {
            Id = Guid.NewGuid(),
        };

        _mapper.Map(request, plant);

        _connection.Insert(plant);

        return new PlantId(plant.Id);
    }

    public Plant[] GetPlants()
    {
        return _connection.Table<DbPlant>().Select(_mapper.Map<Plant>).ToArray();
    }

    public Plant? GetPlantById(PlantId id)
    {
        var result = _connection.Table<DbPlant>().FirstOrDefault(x => x.Id == id.Value);

        if(result == null)
        {
            return null;
        }

        return _mapper.Map<Plant>(result);
    }

    public PlantImage? GetPlantImageById(PlantId id)
    {
        if(_images.TryGetValue(id, out var image))
        {
            return _mapper.Map<PlantImage>(image);
        }

        var result = _connection.Table<DbPlantImage>().FirstOrDefault(x => x.PlantId == id.Value);

        _images.Add(id, result);

        return _mapper.Map<PlantImage>(result);
    }

    public void SetPlantImageUrl(PlantId id, string url)
    {
        var result = GetPlantImageById(id);

        var plantImage = new DbPlantImage() { PlantId = id.Value, Url = url };

        if(result == null)
        {
            _connection.Insert(plantImage);
        }
        else
        {
            _connection.Update(plantImage);
        }

        _images[id] = plantImage;
    }

    public void WaterPlantById(PlantId plantId)
    {
        var now = DateTime.Now;
        var date = now.Date;

        var plant = _connection.Get<DbPlant>(plantId.Value);

        var evt = new DbPlantEvent()
        {
            Id = Guid.NewGuid(),
            PlantId = plant.Id,
            Timestamp = now.ToUniversalTime(),
            Type = PlantEventType.Watering
        };
        _connection.Insert(evt);

        plant.LastWateringDate = date;
        plant.NextWateringDate = date.AddDays(plant.WateringIntervalInDays);

        _connection.Update(plant);
    }

    public void SnoozePlantById(PlantId plantId)
    {
        // TODO validation? Becuase now you can snooze a plant in the future to get it on your list tomorrow. Silly?
        var now = DateTime.Now;
        var date = now.Date;

        var plant = _connection.Get<DbPlant>(plantId.Value);

        var evt = new DbPlantEvent()
        {
            Id = Guid.NewGuid(),
            PlantId = plant.Id,
            Timestamp = now.ToUniversalTime(),
            Type = PlantEventType.Snoozed
        };
        _connection.Insert(evt);

        plant.NextWateringDate = date.AddDays(1);

        _connection.Update(plant);
    }

    public void UpdatePlant(PlantId id, UpdatePlantRequest request)
    {
        var plant = _connection.Get<DbPlant>(id.Value);

        _mapper.Map(request, plant);

        if (plant.LastWateringDate.HasValue)
        {
            plant.NextWateringDate = plant.LastWateringDate.Value.AddDays(plant.WateringIntervalInDays);
        }

        _connection.Update(plant);
    }

    public void DeletePlant(PlantId id)
    {
        _connection.Delete<DbPlant>(id.Value);

        try
        {
            _connection.Execute($"DELETE FROM {nameof(DbPlantEvent)} WHERE {nameof(DbPlantEvent.PlantId)} == ?", id.Value);
            _connection.Execute($"DELETE FROM {nameof(DbPlantImage)} WHERE {nameof(DbPlantImage.PlantId)} == ?", id.Value);
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public void SeedDatabase()
    {
        CreatePlant(new CreatePlantRequest() { Name = "Pannekoekplant", Location = "Op de kast, beneden", WateringIntervalInDays = 4, LatinName = "" });
        CreatePlant(new CreatePlantRequest() { Name = "Bonsai", Location = "Raamkozijn, boven", WateringIntervalInDays = 7, LatinName = "" });
    }
}