namespace PixelLab.Models;

public class ImageInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public long StorageSize { get; set; } // In bytes
    public int TotalPixels { get; set; }

    public string FormattedStorageSize
    {
        get
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = StorageSize;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
