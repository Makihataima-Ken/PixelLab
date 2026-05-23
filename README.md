# PixelLab

**PixelLab** is a comprehensive, interactive Multimedia Systems desktop application built with C# and WPF (.NET 8.0). It is designed to help users visually and intuitively understand image color systems, channel isolation, real-time image processing, and color space quantization.

## 🚀 Features

### 1. Image Import and Display
- **Open Image:** Standard file dialog for loading common formats (PNG, JPG, JPEG, BMP).
- **Drag & Drop:** Load images simply by dragging them from your file explorer into the application window.
- **Preview:** The processed image is displayed live with adjustments.

### 2. Detailed Image Information
Extracts and displays useful metadata:
- File name & Format
- Image Dimensions (Width x Height)
- Formatted Storage Size (e.g., KB, MB)
- Total Pixels

### 3. Multiple Color Systems
Visualizes mathematical color representation by converting internal RGB pixels into:
- **RGB** (Red, Green, Blue)
- **HSV** (Hue, Saturation, Value)
- **CMYK** (Cyan, Magenta, Yellow, Key/Black)
- **YUV** (Luma, Chrominance U, Chrominance V)
- **YCbCr** (Luma, Blue Chroma, Red Chroma)
- **LAB** (Lightness, Green-Red, Blue-Yellow)

### 4. Real-time Channel Control
Select a color system to dynamically update the control sliders. 
- Enable or disable specific channels via checkboxes to observe the effect of missing color data.
- Adjust sliders to scale or shift channel values.
- All modifications process in **real-time** on the loaded image.

### 5. Color Quantization
A uniform color quantization algorithm allows reducing the color depth of the image dynamically. By selecting the number of "Levels", you decrease the bit depth per channel, allowing visualization of low-color palettes (e.g., simulating retro graphics).

### 6. 3D RGB Color Cube (HelixToolkit)
Navigate to the **3D Color Cube** tab to explore a fully interactive, 3D spatial representation of the RGB color model.
- Rotate and zoom the color cube.
- **Click to Inspect:** Click on any sphere in the cube to see its mathematically converted values across all supported color systems (RGB, HSV, CMYK, YUV, YCbCr, LAB).

### 7. Core Tools
- **Save Image:** Export your current processed image to disk as a PNG or JPG.
- **Reset:** Restore the original image and set all values back to default.

---

## 🛠️ Architecture and Technical Notes

The project relies on the **MVVM (Model-View-ViewModel)** design pattern using the `CommunityToolkit.Mvvm` library to ensure clean separation of logic and UI.

### Key Components:
- **`ColorConversionService`:** A static utility service consisting of pure mathematical conversions bridging `RGB` to other spatial definitions. Formats are converted per pixel safely preventing structural breakdown.
- **`ImageProcessingService`:** Directly iterates over a copied `WriteableBitmap`'s byte buffer via 1D array extraction for speed. This avoids costly memory locking and UI blocking.
- **`ColorQuantizationService`:** Performs a uniform bit-crushing effect per channel.
- **`HelixToolkit.Wpf`:** Provides hardware-accelerated 3D rendering for the interactive Color Cube.

---

## ⚙️ How to Run

1. Open `PixelLab.sln` or the `PixelLab` folder in **Visual Studio 2022** (or Rider).
2. Ensure you have the **.NET 8.0 SDK** installed.
3. Build the project to restore NuGet packages (`CommunityToolkit.Mvvm` and `HelixToolkit.Wpf`).
4. Run the application (`F5`).

---

## 📝 Limitations & Future Improvements
- **Performance:** Very high-resolution images (e.g., 4K+) might experience slight stuttering when using real-time sliders due to raw CPU pixel manipulation. Implementing a compute shader (HLSL) or Parallel processing would drastically increase throughput.
- **Quantization:** Uniform quantization is extremely fast but produces visual banding. Implementing *K-Means Clustering* or *Median Cut* algorithms would yield perceptually superior palettes.
- **UI Responsiveness:** A large image drag-and-drop loads synchronously. Adding asynchronous file I/O loading would prevent UI freezing during large file buffering.
