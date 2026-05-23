using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace PixelLab.Services;

public class ImageService
{
    public WriteableBitmap? LoadImage()
    {
        OpenFileDialog dialog = new();

        dialog.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp";

        if (dialog.ShowDialog() != true)
            return null;

        BitmapImage bitmapImage = new();

        using FileStream stream = new(dialog.FileName, FileMode.Open);

        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = stream;
        bitmapImage.EndInit();

        bitmapImage.Freeze();

        // Convert to WriteableBitmap
        WriteableBitmap writeableBitmap = new(bitmapImage);

        return writeableBitmap;
    }
}