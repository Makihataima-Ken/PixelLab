using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace PixelLab.Services;

public class ImageService
{
    public BitmapImage? LoadImage()
    {
        OpenFileDialog dialog = new();

        dialog.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp";

        if (dialog.ShowDialog() != true)
            return null;

        BitmapImage bitmap = new();

        using FileStream stream = new(dialog.FileName, FileMode.Open);

        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();

        bitmap.Freeze();

        return bitmap;
    }
}