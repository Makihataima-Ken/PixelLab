using System.Windows.Media.Imaging;

namespace PixelLab.Models;

public class ImageData
{
    public WriteableBitmap? OriginalImage { get; set; }

    public WriteableBitmap? WorkingImage { get; set; }
}