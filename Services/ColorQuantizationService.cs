using System;

namespace PixelLab.Services;

public class ColorQuantizationService
{
    public static void ApplyUniformQuantization(ref byte r, ref byte g, ref byte b, int colorCount)
    {
        if (colorCount <= 0)
            return;

        colorCount = Math.Clamp(colorCount, 2, 256);
        (int redLevels, int greenLevels, int blueLevels) = GetChannelLevelCounts(colorCount);

        r = QuantizeChannel(r, redLevels);
        g = QuantizeChannel(g, greenLevels);
        b = QuantizeChannel(b, blueLevels);
    }

    private static (int Red, int Green, int Blue) GetChannelLevelCounts(int colorCount)
    {
        int bits = Math.Clamp((int)Math.Round(Math.Log2(colorCount)), 1, 8);
        int redBits = bits / 3;
        int greenBits = bits / 3;
        int blueBits = bits / 3;
        int remainder = bits % 3;

        if (remainder >= 1) greenBits++;
        if (remainder >= 2) redBits++;

        return (1 << redBits, 1 << greenBits, 1 << blueBits);
    }

    private static byte QuantizeChannel(byte value, int levelCount)
    {
        if (levelCount <= 1)
            return 128;

        if (levelCount >= 256)
            return value;

        int level = (int)Math.Round(value / 255.0 * (levelCount - 1));
        int quantizedValue = (int)Math.Round(level * 255.0 / (levelCount - 1));

        return (byte)Math.Clamp(quantizedValue, 0, 255);
    }
}
