using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelLab.ImageProcessing;

public class HsvProcessor
{
    public WriteableBitmap ApplyHsvAdjustments(
        WriteableBitmap source,
        double hueShift,
        double saturationScale,
        double valueScale)
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

            RgbToHsv(r, g, b,
                out double h,
                out double s,
                out double v);

            h += hueShift;

            if (h > 360)
                h -= 360;

            if (h < 0)
                h += 360;

            s *= saturationScale;
            v *= valueScale;

            s = Math.Clamp(s, 0, 1);
            v = Math.Clamp(v, 0, 1);

            HsvToRgb(h, s, v,
                out byte newR,
                out byte newG,
                out byte newB);

            pixels[i] = newB;
            pixels[i + 1] = newG;
            pixels[i + 2] = newR;
        }

        WriteableBitmap result = new(
            width,
            height,
            bitmap.DpiX,
            bitmap.DpiY,
            PixelFormats.Bgra32,
            null);

        result.WritePixels(
            new Int32Rect(0, 0, width, height),
            pixels,
            stride,
            0);

        return result;
    }

    private void RgbToHsv(
        byte r,
        byte g,
        byte b,
        out double h,
        out double s,
        out double v)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));

        double delta = max - min;

        h = 0;

        if (delta != 0)
        {
            if (max == rd)
                h = 60 * (((gd - bd) / delta) % 6);
            else if (max == gd)
                h = 60 * (((bd - rd) / delta) + 2);
            else
                h = 60 * (((rd - gd) / delta) + 4);
        }

        if (h < 0)
            h += 360;

        s = max == 0 ? 0 : delta / max;

        v = max;
    }

    private void HsvToRgb(
        double h,
        double s,
        double v,
        out byte r,
        out byte g,
        out byte b)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double rd = 0;
        double gd = 0;
        double bd = 0;

        if (h < 60)
        {
            rd = c;
            gd = x;
        }
        else if (h < 120)
        {
            rd = x;
            gd = c;
        }
        else if (h < 180)
        {
            gd = c;
            bd = x;
        }
        else if (h < 240)
        {
            gd = x;
            bd = c;
        }
        else if (h < 300)
        {
            rd = x;
            bd = c;
        }
        else
        {
            rd = c;
            bd = x;
        }

        r = (byte)((rd + m) * 255);
        g = (byte)((gd + m) * 255);
        b = (byte)((bd + m) * 255);
    }
}