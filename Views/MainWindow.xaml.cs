using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using PixelLab.ViewModels;
using PixelLab.Models;
using System.Windows.Input;
using System.Linq;
using System.Windows.Threading;

namespace PixelLab;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Point _mouseDownPosition;
    private bool _isLeftClickCandidate;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var initialSystem = ColorSystemType.RGB;
        if (ColorSystemComboBox.SelectedItem is ColorSystemType selectedSystem)
        {
            initialSystem = selectedSystem;
        }

        GenerateColorCube(initialSystem);
    }

    private void ColorSystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ColorSystemComboBox.SelectedItem is ColorSystemType systemType)
        {
            GenerateColorCube(systemType);
        }
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
                            PixelLab.Services.ColorConversionService.RgbToHsv(red, green, blue, out double h, out double s, out double v);
                            double angle = h * Math.PI / 180.0;
                            double radius = s * 5.0;
                            x = radius * Math.Cos(angle);
                            y = radius * Math.Sin(angle);
                            z = v * 10.0;
                            break;
                        case ColorSystemType.CMYK:
                            PixelLab.Services.ColorConversionService.RgbToCmyk(red, green, blue, out double c, out double m, out double yCmyk, out double k);
                            x = c * 10.0;
                            y = m * 10.0;
                            z = yCmyk * 10.0;
                            break;
                        case ColorSystemType.YUV:
                            PixelLab.Services.ColorConversionService.RgbToYuv(red, green, blue, out double yYuv, out double u, out double vYuv);
                            x = u / 10.0;
                            y = vYuv / 10.0;
                            z = yYuv / 25.5;
                            break;
                        case ColorSystemType.YCbCr:
                            PixelLab.Services.ColorConversionService.RgbToYCbCr(red, green, blue, out double yCbCr, out double cb, out double cr);
                            x = (cb - 128) / 10.0;
                            y = (cr - 128) / 10.0;
                            z = yCbCr / 25.5;
                            break;
                        case ColorSystemType.LAB:
                            PixelLab.Services.ColorConversionService.RgbToLab(red, green, blue, out double l, out double a, out double bLab);
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

                    sphere.SetName($"{red},{green},{blue}");

                    ColorCubeModel.Children.Add(sphere);
                }
            }
        }

        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            ColorCubeViewport.ZoomExtents();
        }));
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

    private void HelixViewport3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not HelixViewport3D viewport)
        {
            _isLeftClickCandidate = false;
            return;
        }

        _mouseDownPosition = e.GetPosition(viewport);
        _isLeftClickCandidate = true;
    }

    private void HelixViewport3D_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not HelixViewport3D viewport)
        {
            _isLeftClickCandidate = false;
            return;
        }

        if (!_isLeftClickCandidate)
        {
            return;
        }

        var releasePosition = e.GetPosition(viewport);
        var movement = releasePosition - _mouseDownPosition;
        const double clickThreshold = 4.0;
        if (Math.Abs(movement.X) > clickThreshold || Math.Abs(movement.Y) > clickThreshold)
        {
            _isLeftClickCandidate = false;
            return;
        }

        _isLeftClickCandidate = false;

        var hit = viewport.Viewport.FindHits(releasePosition).FirstOrDefault();
        if (hit != null && hit.Visual is Visual3D visual)
        {
            var name = visual.GetName();
            if (!string.IsNullOrEmpty(name))
            {
                var parts = name.Split(',');
                if (parts.Length == 3
                    && byte.TryParse(parts[0], out var r)
                    && byte.TryParse(parts[1], out var g)
                    && byte.TryParse(parts[2], out var b))
                {
                    if (DataContext is MainViewModel vm)
                    {
                        vm.UpdateSelectedColor(r, g, b);
                    }
                }
            }
        }
    }
}
