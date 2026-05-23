using CommunityToolkit.Mvvm.ComponentModel;

namespace PixelLab.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string appTitle = "PixelLab";
}