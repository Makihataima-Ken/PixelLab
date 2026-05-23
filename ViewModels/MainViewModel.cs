using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixelLab.ImageProcessing;
using PixelLab.Services;
using System.Windows.Media.Imaging;

namespace PixelLab.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ImageService _imageService;
    private readonly RgbProcessor _rgbProcessor;

    private WriteableBitmap? _originalImage;


    [ObservableProperty]
    private WriteableBitmap? displayedImage;

    [ObservableProperty]
    private double redMultiplier = 1.0;

    [ObservableProperty]
    private double greenMultiplier = 1.0;

    [ObservableProperty]
    private double blueMultiplier = 1.0;

    public MainViewModel()
    {
        _imageService = new ImageService();
        _rgbProcessor = new RgbProcessor();
    }
    
    [RelayCommand]
    private void LoadImage()
    {
        WriteableBitmap? image = _imageService.LoadImage();

        if (image == null)
            return;

        _originalImage = image;

        DisplayedImage = image;
    }

    [RelayCommand]
    private void ResetImage()
    {
        if (_originalImage == null)
            return;

        RedMultiplier = 1;
        GreenMultiplier = 1;
        BlueMultiplier = 1;

        DisplayedImage = _originalImage;
    }

    private void ApplyRgbAdjustments()
    {
        if (_originalImage == null)
            return;

        DisplayedImage = _rgbProcessor.ApplyRgbScaling(
            _originalImage,
            RedMultiplier,
            GreenMultiplier,
            BlueMultiplier);
    }

    partial void OnRedMultiplierChanged(double value)
    {
        ApplyRgbAdjustments();
    }

    partial void OnGreenMultiplierChanged(double value)
    {
        ApplyRgbAdjustments();
    }

    partial void OnBlueMultiplierChanged(double value)
    {
        ApplyRgbAdjustments();
    }
}