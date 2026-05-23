using PixelLab.Models;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelLab.Services;

public class ImageProcessingService
{
    public WriteableBitmap ApplyAdjustments(WriteableBitmap source, ColorSystemSettings settings)
    {
        WriteableBitmap bitmap = source.Clone();

        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;

        byte[] pixels = new byte[height * stride];
        bitmap.CopyPixels(pixels, stride, 0);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];
            byte a = pixels[i + 3];

            ProcessPixel(ref r, ref g, ref b, settings);

            pixels[i] = b;
            pixels[i + 1] = g;
            pixels[i + 2] = r;
        }

        WriteableBitmap result = new(
            width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);

        result.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

        return result;
    }

    private void ProcessPixel(ref byte r, ref byte g, ref byte b, ColorSystemSettings settings)
    {
        switch (settings.CurrentSystem)
        {
            case ColorSystemType.RGB:
                ProcessRgb(ref r, ref g, ref b, settings);
                break;
            case ColorSystemType.HSV:
                ProcessHsv(ref r, ref g, ref b, settings);
                break;
            case ColorSystemType.CMYK:
                ProcessCmyk(ref r, ref g, ref b, settings);
                break;
            case ColorSystemType.YUV:
                ProcessYuv(ref r, ref g, ref b, settings);
                break;
            case ColorSystemType.YCbCr:
                ProcessYCbCr(ref r, ref g, ref b, settings);
                break;
            case ColorSystemType.LAB:
                ProcessLab(ref r, ref g, ref b, settings);
                break;
        }

        if (settings.QuantizationLevel < 256)
        {
            ColorQuantizationService.ApplyUniformQuantization(ref r, ref g, ref b, settings.QuantizationLevel);
        }
    }

    private void ProcessRgb(ref byte r, ref byte g, ref byte b, ColorSystemSettings s)
    {
        double nr = s.Channel1Enabled ? r * s.Channel1Value : 0;
        double ng = s.Channel2Enabled ? g * s.Channel2Value : 0;
        double nb = s.Channel3Enabled ? b * s.Channel3Value : 0;

        r = (byte)System.Math.Clamp(nr, 0, 255);
        g = (byte)System.Math.Clamp(ng, 0, 255);
        b = (byte)System.Math.Clamp(nb, 0, 255);
    }

    private void ProcessHsv(ref byte r, ref byte g, ref byte b, ColorSystemSettings s)
    {
        ColorConversionService.RgbToHsv(r, g, b, out double h, out double sat, out double v);

        if (!s.Channel1Enabled) h = 0; else { h += s.Channel1Value; h = (h % 360 + 360) % 360; }
        if (!s.Channel2Enabled) sat = 0; else sat = System.Math.Clamp(sat * s.Channel2Value, 0, 1);
        if (!s.Channel3Enabled) v = 0; else v = System.Math.Clamp(v * s.Channel3Value, 0, 1);

        ColorConversionService.HsvToRgb(h, sat, v, out r, out g, out b);
    }

    private void ProcessCmyk(ref byte r, ref byte g, ref byte b, ColorSystemSettings s)
    {
        ColorConversionService.RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);

        if (!s.Channel1Enabled) c = 0; else c = System.Math.Clamp(c * s.Channel1Value, 0, 1);
        if (!s.Channel2Enabled) m = 0; else m = System.Math.Clamp(m * s.Channel2Value, 0, 1);
        if (!s.Channel3Enabled) y = 0; else y = System.Math.Clamp(y * s.Channel3Value, 0, 1);
        if (!s.Channel4Enabled) k = 0; else k = System.Math.Clamp(k * s.Channel4Value, 0, 1);

        ColorConversionService.CmykToRgb(c, m, y, k, out r, out g, out b);
    }

    private void ProcessYuv(ref byte r, ref byte g, ref byte b, ColorSystemSettings s)
    {
        ColorConversionService.RgbToYuv(r, g, b, out double y, out double u, out double v);

        if (!s.Channel1Enabled) y = 0; else y *= s.Channel1Value;
        if (!s.Channel2Enabled) u = 0; else u *= s.Channel2Value;
        if (!s.Channel3Enabled) v = 0; else v *= s.Channel3Value;

        ColorConversionService.YuvToRgb(y, u, v, out r, out g, out b);
    }

    private void ProcessYCbCr(ref byte r, ref byte g, ref byte b, ColorSystemSettings s)
    {
        ColorConversionService.RgbToYCbCr(r, g, b, out double y, out double cb, out double cr);

        if (!s.Channel1Enabled) y = 0; else y *= s.Channel1Value;
        
        // Cb and Cr are offset by 128
        if (!s.Channel2Enabled) cb = 128; else cb = 128 + (cb - 128) * s.Channel2Value;
        if (!s.Channel3Enabled) cr = 128; else cr = 128 + (cr - 128) * s.Channel3Value;

        ColorConversionService.YCbCrToRgb(y, cb, cr, out r, out g, out b);
    }

    private void ProcessLab(ref byte r, ref byte g, ref byte b, ColorSystemSettings s)
    {
        ColorConversionService.RgbToLab(r, g, b, out double l, out double a, out double bLab);

        if (!s.Channel1Enabled) l = 0; else l *= s.Channel1Value;
        if (!s.Channel2Enabled) a = 0; else a *= s.Channel2Value;
        if (!s.Channel3Enabled) bLab = 0; else bLab *= s.Channel3Value;

        ColorConversionService.LabToRgb(l, a, bLab, out r, out g, out b);
    }
}
