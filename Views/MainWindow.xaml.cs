using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using PixelLab.ViewModels;
using System.Windows.Input;
using System.Linq;

namespace PixelLab;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        GenerateColorCube();
    }

    private void GenerateColorCube()
    {
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

                    var sphere = new SphereVisual3D
                    {
                        Center = new Point3D(red / 25.5, green / 25.5, blue / 25.5),
                        Radius = 0.4,
                        Fill = new SolidColorBrush(color)
                    };

                    sphere.SetName($"{red},{green},{blue}");

                    ColorCubeModel.Children.Add(sphere);
                }
            }
        }
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
                var name = visual.GetName();
                if (!string.IsNullOrEmpty(name))
                {
                    var parts = name.Split(',');
                    if (parts.Length == 3)
                    {
                        byte r = byte.Parse(parts[0]);
                        byte g = byte.Parse(parts[1]);
                        byte b = byte.Parse(parts[2]);

                        if (DataContext is MainViewModel vm)
                        {
                            vm.UpdateSelectedColor(r, g, b);
                        }
                    }
                }
            }
        }
    }
}