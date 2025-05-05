using Microsoft.Maui.Graphics.Platform;
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

    private Plant[]? _plants;
    private Plant? _selectedPlant;
    private Plant? _editingPlant;

    public Plant[]? Plants => _plants;
    public Plant? SelectedPlant => _selectedPlant;
    public Plant? EditingPlant => _editingPlant;

    public ApplicationState(PlantRepository repository)
    {
        _repository = repository;
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

    public async Task<PlantImage> GetPlantImageByIdAsync(PlantId plantId)
    {
        var plantImage = await _repository.GetPlantImageByIdAsync(plantId);

        if(plantImage == null)
        {
            return PlantImage.Placeholder;
        }

        return plantImage;
    }

    public async Task SetPlantImageStream(PlantId plantId, Stream fileStream)
    {
        try
        {
            using var image = PlatformImage.FromStream(fileStream);

            using var downsized = image.Downsize(600);
            
            using var memoryStream = new MemoryStream();

            await downsized.SaveAsync(memoryStream, ImageFormat.Jpeg, 0.8f);

            string base64 = Convert.ToBase64String(memoryStream.ToArray());

            var dataUrl = $"data:image/png;base64,{base64}";

            Debug.WriteLine(dataUrl.Length);

            await SetPlantImageUrlAsync(plantId, dataUrl);
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

    }

    public async Task SetPlantImageUrlAsync(PlantId plantId, string url)
    {
        await _repository.SetPlantImageUrlAsync(plantId, url);

        await RefreshAsync();
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
