using AutoMapper;
using SQLite;
using System.Diagnostics;

namespace SnoozyPlants.Core;

public class PlantRepository 
{
    private bool _databaseCreated;
    private SQLiteConnection _connection;

    private IMapper _mapper;

    private record DataQueryResult<T>
    {
        public T Data { get; set; }
    }

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

    public async Task<Guid?> GetPlantImageVersionAsync(PlantId id)
    {
        var result = _connection.Query<DataQueryResult<Guid>>("SELECT Version as Data FROM DbPlantImage WHERE PlantId = ?", [id.Value]).Select(x => x.Data).ToArray();

        if (result.Length == 0) return null;

        return result[0];
    }

    public async Task<PlantImage?> GetPlantImageAsync(PlantId id)
    {
        var result = _connection.Table<DbPlantImage>().Where(x => x.PlantId == id.Value).FirstOrDefault();

        if(result == null)
        {
            return null;
        }

        return new PlantImage()
        {
            Data = result.Data,
            MimeType = result.MimeType
        };
    }

    public async Task SetPlantImageAsync(PlantId id, PlantImage image)
    {
        var result = _connection.Table<DbPlantImage>().Where(x => x.PlantId == id.Value).FirstOrDefault();

        var plantImage = new DbPlantImage() { PlantId = id.Value, MimeType = image.MimeType, Data = image.Data, Version = Guid.NewGuid() };

        if(result == null)
        {
            _connection.Insert(plantImage);
        }
        else
        {
            _connection.Update(plantImage);
        }
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

    public async Task SetSettingAsync(string key, string value)
    {
        var setting = _connection.Table<DbPlantSetting>().FirstOrDefault(x => x.Key == key);

        if(setting == null)
        {
            setting = new DbPlantSetting()
            {
                Key = key,
                Value = value
            };

            _connection.Insert(setting);
        }
        else
        {
            setting.Value = value;

            _connection.Update(setting);
        }
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var setting = _connection.Table<DbPlantSetting>().FirstOrDefault(x => x.Key == key);

        return setting?.Value;
    }

    public async Task MigrateAsync()
    {
        _connection.CreateTable<DbPlant>();
        _connection.CreateTable<DbPlantEvent>();
        _connection.CreateTable<DbPlantImage>();
        _connection.CreateTable<DbPlantSetting>();

        if (_databaseCreated)
        {
            await SeedDatabaseAsync();
        }

        MigratePlantDataUrls();
    }

    public async Task SeedDatabaseAsync()
    {
        await CreatePlantAsync(new CreatePlantRequest() { Name = "Pannekoekplant", Location = "Op de kast, beneden", WateringIntervalInDays = 4, LatinName = "" });
        await CreatePlantAsync(new CreatePlantRequest() { Name = "Bonsai", Location = "Raamkozijn, boven", WateringIntervalInDays = 7, LatinName = "" });
    }

    public void MigratePlantDataUrls()
    {
        SQLiteConnection.ColumnInfo? result = _connection.Query<SQLiteConnection.ColumnInfo>("SELECT * FROM pragma_table_info('DbPlantImage') WHERE Name = 'Url'", []).FirstOrDefault();

        if(result == null)
        {
            Console.WriteLine("No migration of data urls needed. Column does not exist.");
            return;
        }

        var plantIdsWithImage = _connection.Query<DataQueryResult<Guid>>("SELECT PlantId as Data FROM DbPlantImage").Select(x => x.Data);

        foreach(var plantId in plantIdsWithImage)
        {
            var url = _connection.Query<DataQueryResult<string>>("SELECT Url as Data FROM DbPlantImage WHERE PlantId = ?", [plantId]).Select(x => x.Data).FirstOrDefault();

            if(url == null)
            {
                Console.WriteLine("No data url found for plant... :(");
                continue;
            }

            var data = UrlEncodedBase64ToBytes(url);

            var r = new DbPlantImage()
            {
                PlantId = plantId,
                Data = data,
                MimeType = "image/jpg",
            };

            _connection.Update(r);
        }

        _connection.Execute("ALTER TABLE DbPlantImage DROP COLUMN Url");
    }

    private byte[] UrlEncodedBase64ToBytes(string url)
    {
        const string prefix = "data:image/png;base64,";

        if (!url.StartsWith(prefix))
        {
            throw new Exception("Failed to convert url. Not image/png base64");
        }

        ReadOnlySpan<char> base64Data = url.AsSpan().Slice(prefix.Length);

        byte[] bytes = new byte[base64Data.Length * 6 / 8];

        if (!Convert.TryFromBase64Chars(base64Data, bytes, out int length))
        {
            throw new Exception("Failed to convert base 64 data to binary");
        }

        if(length != bytes.Length)
        {
            Console.WriteLine("Data length mismatch!");
            // Wauw....
            return bytes.AsSpan().Slice(0, length).ToArray();
        }

        return bytes;
    }
}