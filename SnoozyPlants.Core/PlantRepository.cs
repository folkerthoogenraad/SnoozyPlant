using AutoMapper;
using SQLite;
using System.Diagnostics;
using static SQLite.SQLite3;

namespace SnoozyPlants.Core;

public class PlantRepository 
{
    private bool _databaseCreated;
    private SQLiteConnection _connection;

    private IMapper _mapper;

    private Dictionary<PlantId, Task<DbPlantImage?>> _images = new();

    public PlantRepository(PlantDatabaseConfiguration config)
    {
        _databaseCreated = !File.Exists(config.FilePath);

        var options = new SQLiteConnectionString(config.FilePath, true);
        _connection = new SQLiteConnection(options);

        var configuration = new MapperConfiguration(cfg => {
            cfg.AddProfile<PlantAutoMapperProfile>();
        });

        _mapper = configuration.CreateMapper();

        // This is really _amazingly_ stupid, but whatever.
        MigrateAsync().GetAwaiter().GetResult();
    }

    public async Task<PlantId> CreatePlantAsync(CreatePlantRequest request)
    {
        var plant = new DbPlant()
        {
            Id = Guid.NewGuid(),
        };

        _mapper.Map(request, plant);

        _connection.Insert(plant);

        return new PlantId(plant.Id);
    }

    public async Task<Plant[]> GetPlantsAsync()
    {
        return _connection.Table<DbPlant>().Select(_mapper.Map<Plant>).ToArray();
    }

    public async Task<Plant?> GetPlantByIdAsync(PlantId id)
    {
        var result = _connection.Table<DbPlant>().FirstOrDefault(x => x.Id == id.Value);

        if(result == null)
        {
            return null;
        }

        return _mapper.Map<Plant>(result);
    }

    public Task<PlantImage?> GetPlantImageByIdAsync(PlantId id)
    {
        lock (_images)
        {
            if (_images.TryGetValue(id, out var image))
            {
                return MapPlantImage(image);
            }

            var task = LoadImage(id);

            _images.Add(id, task);

            return MapPlantImage(task);
        }
    }

    private async Task<PlantImage?> MapPlantImage(Task<DbPlantImage?> plantImageTask)
    {
        var plantImage = await plantImageTask;

        if(plantImage == null)
        {
            return null;
        }

        return _mapper.Map<PlantImage?>(plantImage);
    }

    private async Task<DbPlantImage?> LoadImage(PlantId plantId)
    {
        var result = _connection.Table<DbPlantImage>().FirstOrDefault(x => x.PlantId == plantId.Value);

        return result;

    }

    public async Task SetPlantImageUrlAsync(PlantId id, string url)
    {
        var result = await GetPlantImageByIdAsync(id);

        var plantImage = new DbPlantImage() { PlantId = id.Value, Url = url };

        if(result == null)
        {
            _connection.Insert(plantImage);
        }
        else
        {
            _connection.Update(plantImage);
        }

        _images[id] = Task.FromResult<DbPlantImage?>(plantImage);
    }

    public async Task WaterPlantByIdAsync(PlantId plantId)
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

    public async Task SnoozePlantByIdAsync(PlantId plantId)
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

    public async Task UpdatePlantAsync(PlantId id, UpdatePlantRequest request)
    {
        var plant = _connection.Get<DbPlant>(id.Value);

        _mapper.Map(request, plant);

        if (plant.LastWateringDate.HasValue)
        {
            plant.NextWateringDate = plant.LastWateringDate.Value.AddDays(plant.WateringIntervalInDays);
        }

        _connection.Update(plant);
    }

    public async Task DeletePlantAsync(PlantId id)
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

    public async Task MigrateAsync()
    {
        _connection.CreateTable<DbPlant>();
        _connection.CreateTable<DbPlantEvent>();
        _connection.CreateTable<DbPlantImage>();

        if (_databaseCreated)
        {
            await SeedDatabaseAsync();
        }
    }

    public async Task SeedDatabaseAsync()
    {
        await CreatePlantAsync(new CreatePlantRequest() { Name = "Pannekoekplant", Location = "Op de kast, beneden", WateringIntervalInDays = 4, LatinName = "" });
        await CreatePlantAsync(new CreatePlantRequest() { Name = "Bonsai", Location = "Raamkozijn, boven", WateringIntervalInDays = 7, LatinName = "" });
    }
}