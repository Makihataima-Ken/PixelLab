using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using HelixToolkit.Wpf;
using PixelLab.ViewModels;
using PixelLab.Models;
using PixelLab.Services;
using System.Windows.Input;
using System.Linq;

namespace PixelLab;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int ColorSpace2DSize = 256;
    private ColorSystemType _currentColorSpaceSystem = ColorSystemType.RGB;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ColorSystemType systemType = ColorSystemType.RGB;
        if (ColorSystemComboBox.SelectedItem is ColorSystemType selectedSystem)
        {
            systemType = selectedSystem;
        }
        else if (DataContext is MainViewModel vm)
        {
            systemType = vm.Settings.CurrentSystem;
            ColorSystemComboBox.SelectedItem = systemType;
        }

        GenerateVisualizations(systemType);
    }

    private void ColorSystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ColorSystemComboBox.SelectedItem is ColorSystemType systemType)
        {
            GenerateVisualizations(systemType);
        }
    }

    private void GenerateVisualizations(ColorSystemType systemType)
    {
        GenerateColorCube(systemType);
        GenerateColorSpace2D(systemType);
    }

    private void GenerateColorCube(ColorSystemType systemType)
    {
        ColorCubeModel.Children.Clear();
        int steps = 8;
        double stepSize = 255.0 / (steps - 1);

        for (int r = 0; r < steps; r++)
        {
            for (int g = 0; g < steps; g++)
            {
                for (int b = 0; b < steps; b++)
                {
                    byte red = (byte)(r * stepSize);
                    byte green = (byte)(g * stepSize);
                    byte blue = (byte)(b * stepSize);

                    var color = Color.FromRgb(red, green, blue);
                    
                    double x = 0, y = 0, z = 0;

                    switch (systemType)
                    {
                        case ColorSystemType.RGB:
                            x = red / 25.5;
                            y = green / 25.5;
                            z = blue / 25.5;
                            break;
                        case ColorSystemType.HSV:
                            ColorConversionService.RgbToHsv(red, green, blue, out double h, out double s, out double v);
                            double angle = h * Math.PI / 180.0;
                            double radius = s * 5.0;
                            x = radius * Math.Cos(angle);
                            y = radius * Math.Sin(angle);
                            z = v * 10.0;
                            break;
                        case ColorSystemType.CMYK:
                            ColorConversionService.RgbToCmyk(red, green, blue, out double c, out double m, out double yCmyk, out double k);
                            x = c * 10.0;
                            y = m * 10.0;
                            z = yCmyk * 10.0;
                            break;
                        case ColorSystemType.YUV:
                            ColorConversionService.RgbToYuv(red, green, blue, out double yYuv, out double u, out double vYuv);
                            x = u / 10.0;
                            y = vYuv / 10.0;
                            z = yYuv / 25.5;
                            break;
                        case ColorSystemType.YCbCr:
                            ColorConversionService.RgbToYCbCr(red, green, blue, out double yCbCr, out double cb, out double cr);
                            x = (cb - 128) / 10.0;
                            y = (cr - 128) / 10.0;
                            z = yCbCr / 25.5;
                            break;
                        case ColorSystemType.LAB:
                            ColorConversionService.RgbToLab(red, green, blue, out double l, out double a, out double bLab);
                            x = a / 10.0;
                            y = bLab / 10.0;
                            z = l / 10.0;
                            break;
                    }

                    var sphere = new SphereVisual3D
                    {
                        Center = new Point3D(x, y, z),
                        Radius = 0.4,
                        Fill = new SolidColorBrush(color)
                    };

                    AttachedProperties.SetName(sphere, $"{red},{green},{blue}");

                    ColorCubeModel.Children.Add(sphere);
                }
            }
        }
    }

    private void GenerateColorSpace2D(ColorSystemType systemType)
    {
        _currentColorSpaceSystem = systemType;
        ColorSpace2DDescription.Text = GetColorSpace2DDescription(systemType);

        int stride = ColorSpace2DSize * 4;
        byte[] pixels = new byte[ColorSpace2DSize * stride];

        for (int y = 0; y < ColorSpace2DSize; y++)
        {
            double normalizedY = 1.0 - y / (double)(ColorSpace2DSize - 1);

            for (int x = 0; x < ColorSpace2DSize; x++)
            {
                double normalizedX = x / (double)(ColorSpace2DSize - 1);
                Color color = GetColorFor2DSlice(systemType, normalizedX, normalizedY);
                int index = y * stride + x * 4;

                pixels[index] = color.B;
                pixels[index + 1] = color.G;
                pixels[index + 2] = color.R;
                pixels[index + 3] = 255;
            }
        }

        WriteableBitmap bitmap = new(
            ColorSpace2DSize,
            ColorSpace2DSize,
            96,
            96,
            PixelFormats.Bgra32,
            null);

        bitmap.WritePixels(
            new Int32Rect(0, 0, ColorSpace2DSize, ColorSpace2DSize),
            pixels,
            stride,
            0);

        ColorSpace2DImage.Source = bitmap;
    }

    private static string GetColorSpace2DDescription(ColorSystemType systemType)
    {
        return systemType switch
        {
            ColorSystemType.RGB => "RGB: X = red, Y = green, blue fixed at 50%.",
            ColorSystemType.HSV => "HSV: X = hue, Y = saturation, value fixed at 100%.",
            ColorSystemType.CMYK => "CMYK: X = cyan, Y = magenta, yellow and key fixed at 0%.",
            ColorSystemType.YUV => "YUV: X = U chroma, Y = V chroma, luma fixed at 50%.",
            ColorSystemType.YCbCr => "YCbCr: X = Cb, Y = Cr, luma fixed at 50%.",
            ColorSystemType.LAB => "LAB: X = a axis, Y = b axis, lightness fixed at 60%.",
            _ => string.Empty
        };
    }

    private static Color GetColorFor2DSlice(ColorSystemType systemType, double normalizedX, double normalizedY)
    {
        byte r;
        byte g;
        byte b;

        switch (systemType)
        {
            case ColorSystemType.RGB:
                return Color.FromRgb(ToByte(normalizedX * 255), ToByte(normalizedY * 255), 128);

            case ColorSystemType.HSV:
                ColorConversionService.HsvToRgb(normalizedX * 360.0, normalizedY, 1.0, out r, out g, out b);
                return Color.FromRgb(r, g, b);

            case ColorSystemType.CMYK:
                ColorConversionService.CmykToRgb(normalizedX, normalizedY, 0, 0, out r, out g, out b);
                return Color.FromRgb(r, g, b);

            case ColorSystemType.YUV:
                double u = -111.0 + normalizedX * 222.0;
                double v = -157.0 + normalizedY * 314.0;
                ColorConversionService.YuvToRgb(128, u, v, out r, out g, out b);
                return Color.FromRgb(r, g, b);

            case ColorSystemType.YCbCr:
                ColorConversionService.YCbCrToRgb(128, normalizedX * 255, normalizedY * 255, out r, out g, out b);
                return Color.FromRgb(r, g, b);

            case ColorSystemType.LAB:
                double a = -128.0 + normalizedX * 255.0;
                double bLab = -128.0 + normalizedY * 255.0;
                ColorConversionService.LabToRgb(60, a, bLab, out r, out g, out b);
                return Color.FromRgb(r, g, b);

            default:
                return Colors.Black;
        }
    }

    private static byte ToByte(double value)
    {
        return (byte)Math.Clamp((int)Math.Round(value), 0, 255);
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.LoadImageFromFile(files[0]);
                }
            }
        }
    }

    private void HelixViewport3D_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is HelixViewport3D viewport)
        {
            var hit = viewport.Viewport.FindHits(e.GetPosition(viewport)).FirstOrDefault();
            if (hit != null && hit.Visual is Visual3D visual)
            {
                var name = AttachedProperties.GetName(visual);
                if (!string.IsNullOrEmpty(name))
                {
                    var parts = name.Split(',');
                    if (parts.Length == 3)
                    {
                        if (byte.TryParse(parts[0], out byte r) &&
                            byte.TryParse(parts[1], out byte g) &&
                            byte.TryParse(parts[2], out byte b) &&
                            DataContext is MainViewModel vm)
                        {
                            vm.UpdateSelectedColor(r, g, b);
                        }
                    }
                }
            }
        }
    }

    private void ColorSpace2DImage_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Image image || image.ActualWidth <= 0 || image.ActualHeight <= 0)
            return;

        Point position = e.GetPosition(image);
        double normalizedX = Math.Clamp(position.X / image.ActualWidth, 0, 1);
        double normalizedY = Math.Clamp(1.0 - position.Y / image.ActualHeight, 0, 1);

        Color color = GetColorFor2DSlice(_currentColorSpaceSystem, normalizedX, normalizedY);
        if (DataContext is MainViewModel vm)
        {
            vm.UpdateSelectedColor(color.R, color.G, color.B);
        }
    }
}
