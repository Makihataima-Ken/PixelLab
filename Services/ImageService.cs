using Microsoft.Win32;
using PixelLab.Models;
using System.IO;
using System.Windows.Media.Imaging;

namespace PixelLab.Services;

public class ImageService
{
    public (WriteableBitmap? Image, ImageInfo? Info) LoadImage()
    {
        OpenFileDialog dialog = new()
        {
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp"
        };

        if (dialog.ShowDialog() != true)
            return (null, null);

        return LoadImageFromFile(dialog.FileName);
    }

    public (WriteableBitmap? Image, ImageInfo? Info) LoadImageFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return (null, null);

        try
        {
            BitmapImage bitmapImage = new();
            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            WriteableBitmap writeableBitmap = new(bitmapImage);

            FileInfo fileInfo = new(filePath);
            ImageInfo info = new()
            {
                FileName = fileInfo.Name,
                FilePath = fileInfo.FullName,
                Format = fileInfo.Extension.TrimStart('.').ToUpper(),
                Width = writeableBitmap.PixelWidth,
                Height = writeableBitmap.PixelHeight,
                StorageSize = fileInfo.Length,
                TotalPixels = writeableBitmap.PixelWidth * writeableBitmap.PixelHeight
            };

            return (writeableBitmap, info);
        }
        catch
        {
            return (null, null);
        }
    }

    public bool SaveImage(WriteableBitmap image)
    {
        if (image == null) return false;

        SaveFileDialog dialog = new()
        {
            Filter = "PNG Image|*.png|JPEG Image|*.jpg",
            Title = "Save Processed Image"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                BitmapEncoder encoder = dialog.FilterIndex == 1
                    ? new PngBitmapEncoder()
                    : new JpegBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(image));

                using FileStream stream = new(dialog.FileName, FileMode.Create);
                encoder.Save(stream);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }
}