using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelLab.ViewModels;

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

    private void EditedImage_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Image image ||
            image.Source is not BitmapSource bitmap ||
            image.ActualWidth <= 0 ||
            image.ActualHeight <= 0)
        {
            return;
        }

        Point position = e.GetPosition(image);
        if (!TryMapImagePointToPixel(image, bitmap, position, out int pixelX, out int pixelY))
        {
            HoveredImagePixelPosition.Text = "Move over the image";
            return;
        }

        Color color = ReadPixel(bitmap, pixelX, pixelY);
        HoveredImageColorSwatch.Background = new SolidColorBrush(color);
        HoveredImagePixelPosition.Text = $"X={pixelX}, Y={pixelY}";

        if (DataContext is MainViewModel vm)
        {
            vm.UpdateSelectedColor(color.R, color.G, color.B);
        }
    }

    private void EditedImage_MouseLeave(object sender, MouseEventArgs e)
    {
        HoveredImagePixelPosition.Text = "Move over the image";
    }

    private static bool TryMapImagePointToPixel(
        Image image,
        BitmapSource bitmap,
        Point point,
        out int pixelX,
        out int pixelY)
    {
        pixelX = 0;
        pixelY = 0;

        double sourceWidth = bitmap.Width;
        double sourceHeight = bitmap.Height;
        if (sourceWidth <= 0 || sourceHeight <= 0)
            return false;

        double scale = Math.Min(image.ActualWidth / sourceWidth, image.ActualHeight / sourceHeight);
        double renderedWidth = sourceWidth * scale;
        double renderedHeight = sourceHeight * scale;
        double offsetX = (image.ActualWidth - renderedWidth) / 2;
        double offsetY = (image.ActualHeight - renderedHeight) / 2;

        if (point.X < offsetX ||
            point.Y < offsetY ||
            point.X > offsetX + renderedWidth ||
            point.Y > offsetY + renderedHeight)
        {
            return false;
        }

        double normalizedX = (point.X - offsetX) / renderedWidth;
        double normalizedY = (point.Y - offsetY) / renderedHeight;

        pixelX = Math.Clamp((int)(normalizedX * bitmap.PixelWidth), 0, bitmap.PixelWidth - 1);
        pixelY = Math.Clamp((int)(normalizedY * bitmap.PixelHeight), 0, bitmap.PixelHeight - 1);
        return true;
    }

    private static Color ReadPixel(BitmapSource source, int x, int y)
    {
        BitmapSource bitmap = source.Format == PixelFormats.Bgra32
            ? source
            : new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

        byte[] pixel = new byte[4];
        bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);

        return Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
    }
}
