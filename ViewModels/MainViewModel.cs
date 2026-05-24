using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixelLab.Models;
using PixelLab.Services;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace PixelLab.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private static readonly QuantizationOption OriginalQuantizationOption = new("Original", 0);

    private readonly ImageService _imageService;
    private readonly ImageProcessingService _processingService;

    private WriteableBitmap? _originalImage;

    [ObservableProperty]
    private WriteableBitmap? displayedImage;

    [ObservableProperty]
    private ImageInfo? imageInfoData;

    [ObservableProperty]
    private ColorSystemSettings settings;

    public IEnumerable<ColorSystemType> AvailableSystems => Enum.GetValues<ColorSystemType>();

    public IReadOnlyList<QuantizationOption> QuantizationOptions { get; } = new[]
    {
        OriginalQuantizationOption,
        new QuantizationOption("256 colors", 256),
        new QuantizationOption("128 colors", 128),
        new QuantizationOption("64 colors", 64),
        new QuantizationOption("32 colors", 32),
        new QuantizationOption("16 colors", 16),
        new QuantizationOption("8 colors", 8),
        new QuantizationOption("4 colors", 4),
        new QuantizationOption("2 colors", 2)
    };

    [ObservableProperty]
    private QuantizationOption selectedQuantizationOption = OriginalQuantizationOption;

    public MainViewModel()
    {
        _imageService = new ImageService();
        _processingService = new ImageProcessingService();
        
        Settings = new ColorSystemSettings();
        Settings.SetupForSystem(ColorSystemType.RGB);
        Settings.QuantizationLevel = SelectedQuantizationOption.ColorCount;

        Settings.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != nameof(Settings.CurrentSystem) &&
                e.PropertyName != nameof(Settings.Channel1Name) &&
                e.PropertyName != nameof(Settings.Channel2Name) &&
                e.PropertyName != nameof(Settings.Channel3Name) &&
                e.PropertyName != nameof(Settings.Channel4Name) &&
                e.PropertyName != nameof(Settings.Channel1Min) &&
                e.PropertyName != nameof(Settings.IsChannel4Visible))
            {
                ApplyAdjustments();
            }

            if (e.PropertyName == nameof(Settings.CurrentSystem))
            {
                Settings.SetupForSystem(Settings.CurrentSystem);
                ApplyAdjustments();
            }
        };
    }

    [RelayCommand]
    private void LoadImage()
    {
        var result = _imageService.LoadImage();
        ProcessLoadedImage(result.Image, result.Info);
    }

    public void LoadImageFromFile(string filePath)
    {
        var result = _imageService.LoadImageFromFile(filePath);
        ProcessLoadedImage(result.Image, result.Info);
    }

    private void ProcessLoadedImage(WriteableBitmap? image, ImageInfo? info)
    {
        if (image == null || info == null)
            return;

        _originalImage = image;
        ImageInfoData = info;
        DisplayedImage = image;
        ResetImage();
    }

    [RelayCommand]
    private void SaveImage()
    {
        if (DisplayedImage != null)
        {
            _imageService.SaveImage(DisplayedImage);
        }
    }

    [RelayCommand]
    private void ResetImage()
    {
        if (_originalImage == null)
            return;

        SelectedQuantizationOption = OriginalQuantizationOption;
        Settings.SetupForSystem(ColorSystemType.RGB);
        ApplyAdjustments();
    }

    private void ApplyAdjustments()
    {
        if (_originalImage == null)
            return;

        DisplayedImage = _processingService.ApplyAdjustments(_originalImage, Settings);
    }

    partial void OnSelectedQuantizationOptionChanged(QuantizationOption value)
    {
        Settings.QuantizationLevel = value.ColorCount;
    }

    [ObservableProperty]
    private string selectedColorValues = "No color selected.";

    public void UpdateSelectedColor(byte r, byte g, byte b)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"RGB: R={r}, G={g}, B={b}");

        ColorConversionService.RgbToHsv(r, g, b, out double h, out double s, out double v);
        sb.AppendLine($"HSV: H={h:0.##} deg, S={s * 100:0.##}%, V={v * 100:0.##}%");

        ColorConversionService.RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);
        sb.AppendLine($"CMYK: C={c * 100:0.##}%, M={m * 100:0.##}%, Y={y * 100:0.##}%, K={k * 100:0.##}%");

        ColorConversionService.RgbToYuv(r, g, b, out double yYuv, out double u, out double vYuv);
        sb.AppendLine($"YUV: Y={yYuv:0.##}, U={u:0.##}, V={vYuv:0.##}");

        ColorConversionService.RgbToYCbCr(r, g, b, out double yCbCr, out double cb, out double cr);
        sb.AppendLine($"YCbCr: Y={yCbCr:0.##}, Cb={cb:0.##}, Cr={cr:0.##}");

        ColorConversionService.RgbToLab(r, g, b, out double l, out double a, out double bLab);
        sb.AppendLine($"LAB: L={l:0.##}, a={a:0.##}, b={bLab:0.##}");

        SelectedColorValues = sb.ToString();
    }
}
