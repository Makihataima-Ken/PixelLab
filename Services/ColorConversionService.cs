using System;

namespace PixelLab.Services;

public class ColorConversionService
{
    // --- HSV ---
    public static void RgbToHsv(byte r, byte g, byte b, out double h, out double s, out double v)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;

        h = 0;
        if (delta > 0)
        {
            if (max == rd) h = 60 * (((gd - bd) / delta) % 6);
            else if (max == gd) h = 60 * (((bd - rd) / delta) + 2);
            else h = 60 * (((rd - gd) / delta) + 4);
        }

        if (h < 0) h += 360;
        s = max == 0 ? 0 : delta / max;
        v = max;
    }

    public static void HsvToRgb(double h, double s, double v, out byte r, out byte g, out byte b)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double rd = 0, gd = 0, bd = 0;
        if (h < 60) { rd = c; gd = x; }
        else if (h < 120) { rd = x; gd = c; }
        else if (h < 180) { gd = c; bd = x; }
        else if (h < 240) { gd = x; bd = c; }
        else if (h < 300) { rd = x; bd = c; }
        else { rd = c; bd = x; }

        r = (byte)Math.Clamp((rd + m) * 255, 0, 255);
        g = (byte)Math.Clamp((gd + m) * 255, 0, 255);
        b = (byte)Math.Clamp((bd + m) * 255, 0, 255);
    }

    // --- CMYK ---
    public static void RgbToCmyk(byte r, byte g, byte b, out double c, out double m, out double y, out double k)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        k = 1 - Math.Max(rd, Math.Max(gd, bd));
        if (k == 1)
        {
            c = 0; m = 0; y = 0;
        }
        else
        {
            c = (1 - rd - k) / (1 - k);
            m = (1 - gd - k) / (1 - k);
            y = (1 - bd - k) / (1 - k);
        }
    }

    public static void CmykToRgb(double c, double m, double y, double k, out byte r, out byte g, out byte b)
    {
        r = (byte)Math.Clamp(255 * (1 - c) * (1 - k), 0, 255);
        g = (byte)Math.Clamp(255 * (1 - m) * (1 - k), 0, 255);
        b = (byte)Math.Clamp(255 * (1 - y) * (1 - k), 0, 255);
    }

    // --- YUV ---
    public static void RgbToYuv(byte r, byte g, byte b, out double y, out double u, out double v)
    {
        y = 0.299 * r + 0.587 * g + 0.114 * b;
        u = -0.14713 * r - 0.28886 * g + 0.436 * b;
        v = 0.615 * r - 0.51499 * g - 0.10001 * b;
    }

    public static void YuvToRgb(double y, double u, double v, out byte r, out byte g, out byte b)
    {
        r = (byte)Math.Clamp(y + 1.13983 * v, 0, 255);
        g = (byte)Math.Clamp(y - 0.39465 * u - 0.58060 * v, 0, 255);
        b = (byte)Math.Clamp(y + 2.03211 * u, 0, 255);
    }

    // --- YCbCr ---
    public static void RgbToYCbCr(byte r, byte g, byte b, out double y, out double cb, out double cr)
    {
        y = 0.299 * r + 0.587 * g + 0.114 * b;
        cb = 128 - 0.168736 * r - 0.331264 * g + 0.5 * b;
        cr = 128 + 0.5 * r - 0.418688 * g - 0.081312 * b;
    }

    public static void YCbCrToRgb(double y, double cb, double cr, out byte r, out byte g, out byte b)
    {
        r = (byte)Math.Clamp(y + 1.402 * (cr - 128), 0, 255);
        g = (byte)Math.Clamp(y - 0.344136 * (cb - 128) - 0.714136 * (cr - 128), 0, 255);
        b = (byte)Math.Clamp(y + 1.772 * (cb - 128), 0, 255);
    }

    // --- LAB ---
    private static double PivotXyz(double n)
    {
        return n > 0.008856 ? Math.Pow(n, 1.0 / 3.0) : (7.787 * n + 16.0 / 116.0);
    }

    private static double PivotXyzReverse(double n)
    {
        double n3 = Math.Pow(n, 3);
        return n3 > 0.008856 ? n3 : (n - 16.0 / 116.0) / 7.787;
    }

    public static void RgbToLab(byte r, byte g, byte b, out double l, out double a, out double bLab)
    {
        // RGB to XYZ
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        rd = rd > 0.04045 ? Math.Pow((rd + 0.055) / 1.055, 2.4) : rd / 12.92;
        gd = gd > 0.04045 ? Math.Pow((gd + 0.055) / 1.055, 2.4) : gd / 12.92;
        bd = bd > 0.04045 ? Math.Pow((bd + 0.055) / 1.055, 2.4) : bd / 12.92;

        rd *= 100;
        gd *= 100;
        bd *= 100;

        double x = rd * 0.4124 + gd * 0.3576 + bd * 0.1805;
        double y = rd * 0.2126 + gd * 0.7152 + bd * 0.0722;
        double z = rd * 0.0193 + gd * 0.1192 + bd * 0.9505;

        // XYZ to LAB
        double xr = 95.047;
        double yr = 100.000;
        double zr = 108.883;

        x = PivotXyz(x / xr);
        y = PivotXyz(y / yr);
        z = PivotXyz(z / zr);

        l = Math.Max(0, 116 * y - 16);
        a = 500 * (x - y);
        bLab = 200 * (y - z);
    }

    public static void LabToRgb(double l, double a, double bLab, out byte r, out byte g, out byte b)
    {
        // LAB to XYZ
        double y = (l + 16) / 116.0;
        double x = a / 500.0 + y;
        double z = y - bLab / 200.0;

        double xr = 95.047;
        double yr = 100.000;
        double zr = 108.883;

        x = PivotXyzReverse(x) * xr;
        y = PivotXyzReverse(y) * yr;
        z = PivotXyzReverse(z) * zr;

        x /= 100.0;
        y /= 100.0;
        z /= 100.0;

        // XYZ to RGB
        double rd = x * 3.2406 + y * -1.5372 + z * -0.4986;
        double gd = x * -0.9689 + y * 1.8758 + z * 0.0415;
        double bd = x * 0.0557 + y * -0.2040 + z * 1.0570;

        rd = rd > 0.0031308 ? 1.055 * Math.Pow(rd, 1 / 2.4) - 0.055 : 12.92 * rd;
        gd = gd > 0.0031308 ? 1.055 * Math.Pow(gd, 1 / 2.4) - 0.055 : 12.92 * gd;
        bd = bd > 0.0031308 ? 1.055 * Math.Pow(bd, 1 / 2.4) - 0.055 : 12.92 * bd;

        r = (byte)Math.Clamp(rd * 255, 0, 255);
        g = (byte)Math.Clamp(gd * 255, 0, 255);
        b = (byte)Math.Clamp(bd * 255, 0, 255);
    }
}
