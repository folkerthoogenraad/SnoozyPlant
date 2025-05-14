using Microsoft.Maui.Graphics.Platform;
using SnoozyPlants.App.Server;
using SnoozyPlants.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.App.Model;

internal class ApplicationState
{
    private readonly PlantRepository _repository;
    private readonly PlantImageServer _server;

    private Plant[]? _plants;
    private Plant? _selectedPlant;
    private Plant? _editingPlant;

    public Plant[]? Plants => _plants;
    public Plant? SelectedPlant => _selectedPlant;
    public Plant? EditingPlant => _editingPlant;

    public ApplicationState(PlantRepository repository, PlantImageServer server)
    {
        _repository = repository;
        _server = server;
    }

    public Plant? GetPlantById(PlantId plantId)
    {
        if (_plants == null)
        {
            return null;
        }

        return _plants.FirstOrDefault(plant => plant.Id == plantId);
    }

    public async Task<Plant> CreatePlantAsync(CreatePlantRequest request)
    {
        var id = await _repository.CreatePlantAsync( request);

        await RefreshAsync();

        return GetPlantById(id)!;
    }

    public async Task UpdatePlantAsync(PlantId plantId, UpdatePlantRequest request)
    {
        await _repository.UpdatePlantAsync(plantId, request);

        await RefreshAsync();
    }

    public async Task<string> GetPlantImageUrlByIdAsync(PlantId plantId)
    {
        var versionGuid = await _repository.GetPlantImageVersionAsync(plantId);

        if (!versionGuid.HasValue)
        {
            return "/images/missing.svg";
        }

        return _server.CreateImageUrl(plantId, versionGuid.Value);
    }

    public string GetPlantImageMissingAsync(PlantId plantId)
    {
        return "/images/missing.svg";
    }

    public async Task SetPlantImageStream(PlantId plantId, Stream fileStream)
    {
        try
        {
            using var image = PlatformImage.FromStream(fileStream);

            using var downsized = image.Downsize(1024);
            
            using var memoryStream = new MemoryStream();

            await downsized.SaveAsync(memoryStream, ImageFormat.Jpeg, 0.8f);

            await _repository.SetPlantImageAsync(plantId, new PlantImage()
            {
                Data = memoryStream.ToArray(),
                MimeType = "image/jpg"
            });
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

    }

    public async Task DeletePlantAsync(Plant plant)
    {
        await _repository.DeletePlantAsync(plant.Id);

        _selectedPlant = null;

        await RefreshAsync();
    }

    public async Task ClonePlantAsync(Plant plant)
    {
        var newId = await _repository.CreatePlantAsync(new CreatePlantRequest()
        {
            Name = plant.Name,
            LatinName = plant.LatinName,
            Location = plant.Location
        });

        await RefreshAsync();

        SetSelected(newId);
    }

    public async Task WaterPlantAsync(Plant plant)
    {
        await _repository.WaterPlantByIdAsync(plant.Id);

        await RefreshAsync();

        SetSelected(null);
    }

    public async Task SnoozePlantAsync(Plant plant)
    {
        await _repository.SnoozePlantByIdAsync(plant.Id);

        await RefreshAsync();

        SetSelected(null);
    }

    public void SetSelected(PlantId plantId)
    {
        SetSelected(GetPlantById(plantId));
    }

    public void SetSelected(Plant? selected)
    {
        _selectedPlant = selected;
    }

    public void SetEditing(Plant? editing)
    {
        _editingPlant = editing;
    }

    public async Task RefreshAsync()
    {
        _plants = (await _repository.GetPlantsAsync()).OrderBy(x => x.NextWateringDate).ToArray();

        if (_selectedPlant != null)
        {
            _selectedPlant = GetPlantById(_selectedPlant.Id);
        }

        if (_editingPlant != null)
        {
            _editingPlant = GetPlantById(_editingPlant.Id);
        }
    }
}
