using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelLab.ViewModels;
using PixelLab.Models;
using PixelLab.Services;
using System.Windows.Input;

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

        GenerateColorSpace2D(systemType);
    }

    private void ColorSystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ColorSystemComboBox.SelectedItem is ColorSystemType systemType)
        {
            GenerateColorSpace2D(systemType);
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
        ColorHoverMarker.Visibility = Visibility.Collapsed;
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

    private void ColorSpace2DImage_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Image image || image.ActualWidth <= 0 || image.ActualHeight <= 0)
            return;

        Point position = e.GetPosition(image);
        double normalizedX = Math.Clamp(position.X / image.ActualWidth, 0, 1);
        double normalizedY = Math.Clamp(1.0 - position.Y / image.ActualHeight, 0, 1);

        Color color = GetColorFor2DSlice(_currentColorSpaceSystem, normalizedX, normalizedY);
        ColorHoverMarker.Visibility = Visibility.Visible;
        Canvas.SetLeft(ColorHoverMarker, position.X - ColorHoverMarker.Width / 2);
        Canvas.SetTop(ColorHoverMarker, position.Y - ColorHoverMarker.Height / 2);
        HoveredColorSwatch.Background = new SolidColorBrush(color);

        if (DataContext is MainViewModel vm)
        {
            vm.UpdateSelectedColor(color.R, color.G, color.B);
        }
    }

    private void ColorSpace2DImage_MouseLeave(object sender, MouseEventArgs e)
    {
        ColorHoverMarker.Visibility = Visibility.Collapsed;
    }
}
