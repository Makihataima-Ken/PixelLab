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
    private readonly HsvProcessor _hsvProcessor;

    private WriteableBitmap? _originalImage;


    [ObservableProperty]
    private WriteableBitmap? displayedImage;

    [ObservableProperty]
    private double redMultiplier = 1.0;

    [ObservableProperty]
    private double greenMultiplier = 1.0;

    [ObservableProperty]
    private double blueMultiplier = 1.0;

    [ObservableProperty]
    private double hueShift = 0;

    [ObservableProperty]
    private double saturationMultiplier = 1;

    [ObservableProperty]
    private double valueMultiplier = 1;

    public MainViewModel()
    {
        _imageService = new ImageService();
        _rgbProcessor = new RgbProcessor();
        _hsvProcessor = new HsvProcessor();
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

    private void ApplyHsvAdjustments()
    {
        if (_originalImage == null)
            return;

        DisplayedImage =
            _hsvProcessor.ApplyHsvAdjustments(
                _originalImage,
                HueShift,
                SaturationMultiplier,
                ValueMultiplier);
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


    partial void OnHueShiftChanged(double value)
    {
        ApplyHsvAdjustments();
    }

    partial void OnSaturationMultiplierChanged(double value)
    {
        ApplyHsvAdjustments();
    }

    partial void OnValueMultiplierChanged(double value)
    {
        ApplyHsvAdjustments();
    }
}