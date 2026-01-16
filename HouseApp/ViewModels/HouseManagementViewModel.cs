using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseApp.Services;
using HouseApp.Models;

namespace HouseApp.ViewModels;

public partial class HouseManagementViewModel : ObservableObject
{
    private readonly HouseService _houseService;
    private readonly UserSession _userSession;

    [ObservableProperty]
    private ObservableCollection<HouseDto> houses = new();

    [ObservableProperty]
    private HouseDto? selectedHouse;

    [ObservableProperty]
    private bool isLoading;

    public HouseManagementViewModel(HouseService houseService, UserSession userSession)
    {
        _houseService = houseService;
        _userSession = userSession;
    }

    [RelayCommand]
    private async Task LoadHousesAsync()
    {
        IsLoading = true;
        
        try
        {
            var loadedHouses = await _houseService.GetLandlordHousesAsync(_userSession.UserId);
            Houses = new ObservableCollection<HouseDto>(loadedHouses ?? new List<HouseDto>());
            
            if (Houses.Any())
            {
                SelectedHouse = Houses.First();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (SelectedHouse == null) return;
        
        IsLoading = true;
        
        try
        {
            var success = await _houseService.UpdateHouseAsync(SelectedHouse);
            
            if (success)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Success", 
                    "Changes saved successfully!", 
                    "OK");
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error", 
                    "Failed to save changes. Please try again.", 
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", 
                $"An error occurred: {ex.Message}", 
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteHouseAsync()
    {
        if (SelectedHouse == null) return;

        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete {SelectedHouse.Address}?",
            "Yes",
            "No");

        if (!confirm) return;

        IsLoading = true;

        try
        {
            var success = await _houseService.DeleteHouseAsync(SelectedHouse.HouseId);

            if (success)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Success",
                    "House deleted successfully!",
                    "OK");
                
                await GoBackAsync();
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Error",
                    "Failed to delete house. Please try again.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error",
                $"An error occurred: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
