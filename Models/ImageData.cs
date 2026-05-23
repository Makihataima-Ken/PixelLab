using System.Windows.Media.Imaging;

namespace PixelLab.Models;

public class ImageData
{
    public BitmapImage? OriginalImage { get; set; }

    public BitmapImage? WorkingImage { get; set; }
}