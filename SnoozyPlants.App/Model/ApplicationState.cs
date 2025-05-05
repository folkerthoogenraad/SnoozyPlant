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
        var id = _repository.CreatePlant( request);

        await RefreshAsync();

        return GetPlantById(id)!;
    }

    public async Task UpdatePlantAsync(PlantId plantId, UpdatePlantRequest request)
    {
        _repository.UpdatePlant(plantId, request);

        await RefreshAsync();
    }

    public PlantImage GetPlantImageById(PlantId plantId)
    {
        var plantImage = _repository.GetPlantImageById(plantId);

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

            using var downsized = image.Downsize(256);

            using var memoryStream = new MemoryStream();

            await downsized.SaveAsync(memoryStream, ImageFormat.Jpeg, 0.8f);

            string base64 = Convert.ToBase64String(memoryStream.ToArray());

            var dataUrl = $"data:image/png;base64,{base64}";

            await SetPlantImageUrlAsync(plantId, dataUrl);
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

    }

    public async Task SetPlantImageUrlAsync(PlantId plantId, string url)
    {
        _repository.SetPlantImageUrl(plantId, url);

        await RefreshAsync();
    }

    public async Task DeletePlantAsync(Plant plant)
    {
        _repository.DeletePlant(plant.Id);

        _selectedPlant = null;

        await RefreshAsync();
    }

    public async Task ClonePlantAsync(Plant plant)
    {
        var newId = _repository.CreatePlant(new CreatePlantRequest()
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
        _repository.WaterPlantById(plant.Id);

        await RefreshAsync();

        SetSelected(null);
    }

    public async Task SnoozePlantAsync(Plant plant)
    {
        _repository.SnoozePlantById(plant.Id);

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

    public Task RefreshAsync()
    {
        _plants = _repository.GetPlants().OrderBy(x => x.NextWateringDate).ToArray();

        if (_selectedPlant != null)
        {
            _selectedPlant = GetPlantById(_selectedPlant.Id);
        }

        if (_editingPlant != null)
        {
            _editingPlant = GetPlantById(_editingPlant.Id);
        }

        return Task.CompletedTask;
    }
}
