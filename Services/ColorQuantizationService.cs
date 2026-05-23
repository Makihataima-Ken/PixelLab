using System;

namespace PixelLab.Services;

public class ColorQuantizationService
{
    public static void ApplyUniformQuantization(ref byte r, ref byte g, ref byte b, int levelCount)
    {
        if (levelCount <= 0 || levelCount >= 256)
            return;

        int step = 256 / levelCount;

        r = QuantizeChannel(r, step, levelCount);
        g = QuantizeChannel(g, step, levelCount);
        b = QuantizeChannel(b, step, levelCount);
    }

    private static byte QuantizeChannel(byte value, int step, int levelCount)
    {
        int level = value / step;
        if (level >= levelCount) level = levelCount - 1;

        // Map to the middle of the bin to reduce error, or to min/max
        int quantizedValue = (level * step) + (step / 2);
        
        return (byte)Math.Clamp(quantizedValue, 0, 255);
    }
}
