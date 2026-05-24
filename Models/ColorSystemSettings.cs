using CommunityToolkit.Mvvm.ComponentModel;

namespace PixelLab.Models;

public partial class ColorSystemSettings : ObservableObject
{
    [ObservableProperty]
    private ColorSystemType currentSystem = ColorSystemType.RGB;

    // Channel 1
    [ObservableProperty] private string channel1Name = "R";
    [ObservableProperty] private bool channel1Enabled = true;
    [ObservableProperty] private double channel1Value = 1.0;
    [ObservableProperty] private double channel1Min = 0.0;
    [ObservableProperty] private double channel1Max = 2.0;

    // Channel 2
    [ObservableProperty] private string channel2Name = "G";
    [ObservableProperty] private bool channel2Enabled = true;
    [ObservableProperty] private double channel2Value = 1.0;
    [ObservableProperty] private double channel2Min = 0.0;
    [ObservableProperty] private double channel2Max = 2.0;

    // Channel 3
    [ObservableProperty] private string channel3Name = "B";
    [ObservableProperty] private bool channel3Enabled = true;
    [ObservableProperty] private double channel3Value = 1.0;
    [ObservableProperty] private double channel3Min = 0.0;
    [ObservableProperty] private double channel3Max = 2.0;

    // Channel 4 (For CMYK)
    [ObservableProperty] private string channel4Name = "K";
    [ObservableProperty] private bool channel4Enabled = true;
    [ObservableProperty] private double channel4Value = 1.0;
    [ObservableProperty] private double channel4Min = 0.0;
    [ObservableProperty] private double channel4Max = 2.0;
    [ObservableProperty] private bool isChannel4Visible = false;

    // Quantization
    [ObservableProperty] private int quantizationLevel = 0;

    public void SetupForSystem(ColorSystemType system)
    {
        CurrentSystem = system;
        Channel1Enabled = true;
        Channel2Enabled = true;
        Channel3Enabled = true;
        Channel4Enabled = true;

        switch (system)
        {
            case ColorSystemType.RGB:
                Channel1Name = "R (Red)"; Channel1Min = 0; Channel1Max = 2; Channel1Value = 1;
                Channel2Name = "G (Green)"; Channel2Min = 0; Channel2Max = 2; Channel2Value = 1;
                Channel3Name = "B (Blue)"; Channel3Min = 0; Channel3Max = 2; Channel3Value = 1;
                IsChannel4Visible = false;
                break;
            case ColorSystemType.HSV:
                Channel1Name = "H (Hue Shift)"; Channel1Min = -180; Channel1Max = 180; Channel1Value = 0;
                Channel2Name = "S (Saturation)"; Channel2Min = 0; Channel2Max = 2; Channel2Value = 1;
                Channel3Name = "V (Value)"; Channel3Min = 0; Channel3Max = 2; Channel3Value = 1;
                IsChannel4Visible = false;
                break;
            case ColorSystemType.CMYK:
                Channel1Name = "C (Cyan)"; Channel1Min = 0; Channel1Max = 2; Channel1Value = 1;
                Channel2Name = "M (Magenta)"; Channel2Min = 0; Channel2Max = 2; Channel2Value = 1;
                Channel3Name = "Y (Yellow)"; Channel3Min = 0; Channel3Max = 2; Channel3Value = 1;
                Channel4Name = "K (Key/Black)"; Channel4Min = 0; Channel4Max = 2; Channel4Value = 1;
                IsChannel4Visible = true;
                break;
            case ColorSystemType.YUV:
                Channel1Name = "Y (Luma)"; Channel1Min = 0; Channel1Max = 2; Channel1Value = 1;
                Channel2Name = "U (Chrominance)"; Channel2Min = 0; Channel2Max = 2; Channel2Value = 1;
                Channel3Name = "V (Chrominance)"; Channel3Min = 0; Channel3Max = 2; Channel3Value = 1;
                IsChannel4Visible = false;
                break;
            case ColorSystemType.YCbCr:
                Channel1Name = "Y (Luma)"; Channel1Min = 0; Channel1Max = 2; Channel1Value = 1;
                Channel2Name = "Cb (Blue Chroma)"; Channel2Min = 0; Channel2Max = 2; Channel2Value = 1;
                Channel3Name = "Cr (Red Chroma)"; Channel3Min = 0; Channel3Max = 2; Channel3Value = 1;
                IsChannel4Visible = false;
                break;
            case ColorSystemType.LAB:
                Channel1Name = "L (Lightness)"; Channel1Min = 0; Channel1Max = 2; Channel1Value = 1;
                Channel2Name = "a (Green-Red)"; Channel2Min = 0; Channel2Max = 2; Channel2Value = 1;
                Channel3Name = "b (Blue-Yellow)"; Channel3Min = 0; Channel3Max = 2; Channel3Value = 1;
                IsChannel4Visible = false;
                break;
        }
    }
}
