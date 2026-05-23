using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelLab.ImageProcessing;

public class RgbProcessor
{
    public WriteableBitmap ApplyRgbScaling(
        WriteableBitmap source,
        double rScale,
        double gScale,
        double bScale)
    {
        WriteableBitmap bitmap = source.Clone();

        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;

        byte[] pixels = new byte[height * stride];

        bitmap.CopyPixels(pixels, stride, 0);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = Clamp(pixels[i] * bScale);       // Blue
            pixels[i + 1] = Clamp(pixels[i + 1] * gScale); // Green
            pixels[i + 2] = Clamp(pixels[i + 2] * rScale); // Red
        }

        WriteableBitmap result = new(
            width,
            height,
            bitmap.DpiX,
            bitmap.DpiY,
            PixelFormats.Bgra32,
            null);

        result.WritePixels(
            new System.Windows.Int32Rect(0, 0, width, height),
            pixels,
            stride,
            0);

        return result;
    }

    private byte Clamp(double value)
    {
        return (byte)Math.Max(0, Math.Min(255, value));
    }
}