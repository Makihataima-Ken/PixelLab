using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixelLab.Services;
using System.Windows.Media.Imaging;

namespace PixelLab.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ImageService _imageService;

    private BitmapImage? _originalImage;

    [ObservableProperty]
    private BitmapImage? displayedImage;

    public MainViewModel()
    {
        _imageService = new ImageService();
    }

    [RelayCommand]
    private void LoadImage()
    {
        BitmapImage? image = _imageService.LoadImage();

        if (image == null)
            return;

        _originalImage = image;

        DisplayedImage = image;
    }

    [RelayCommand]
    private void ResetImage()
    {
        DisplayedImage = _originalImage;
    }
}