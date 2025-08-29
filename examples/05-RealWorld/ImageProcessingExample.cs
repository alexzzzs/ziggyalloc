using System;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples.RealWorld
{
    /// <summary>
    /// Example demonstrating real-world image processing using ZiggyAlloc.
    /// Shows how to efficiently process large images without GC pressure.
    /// </summary>
    public class ImageProcessingExample
    {
        public static void Run()
        {
            Console.WriteLine("=== Image Processing Example ===\n");

            var allocator = new SystemMemoryAllocator();
            
            // Simulate a 4K image (3840x2160) with RGBA channels
            const int width = 3840;
            const int height = 2160;
            const int channels = 4; // RGBA
            const int pixelCount = width * height;
            const int totalBytes = pixelCount * channels;
            
            Console.WriteLine($"Processing a 4K image ({width}x{height}) with {channels} channels:");
            Console.WriteLine($"  Total pixels: {pixelCount:N0}");
            Console.WriteLine($"  Total memory: {totalBytes / (1024 * 1024)} MB\n");

            // Allocate image buffer using ZiggyAlloc
            using var imageBuffer = allocator.Allocate<byte>(totalBytes);
            
            // Fill with test pattern
            FillWithTestPattern(imageBuffer, width, height, channels);
            
            Console.WriteLine("1. Image buffer allocated and filled with test pattern");
            Console.WriteLine($"   Buffer size: {imageBuffer.SizeInBytes / (1024 * 1024)} MB");
            Console.WriteLine($"   First 8 bytes: {GetBytesAsString(imageBuffer, 0, 8)}");
            Console.WriteLine($"   Last 8 bytes:  {GetBytesAsString(imageBuffer, imageBuffer.Length - 8, 8)}\n");

            // Apply brightness adjustment
            AdjustBrightness(imageBuffer, 1.2f);
            Console.WriteLine("2. Applied brightness adjustment (1.2x)");
            Console.WriteLine($"   First 8 bytes: {GetBytesAsString(imageBuffer, 0, 8)}");
            Console.WriteLine($"   Last 8 bytes:  {GetBytesAsString(imageBuffer, imageBuffer.Length - 8, 8)}\n");

            // Apply contrast adjustment
            AdjustContrast(imageBuffer, 1.5f);
            Console.WriteLine("3. Applied contrast adjustment (1.5x)");
            Console.WriteLine($"   First 8 bytes: {GetBytesAsString(imageBuffer, 0, 8)}");
            Console.WriteLine($"   Last 8 bytes:  {GetBytesAsString(imageBuffer, imageBuffer.Length - 8, 8)}\n");

            // Convert to grayscale
            ConvertToGrayscale(imageBuffer, width, height);
            Console.WriteLine("4. Converted to grayscale");
            Console.WriteLine($"   First 8 bytes: {GetBytesAsString(imageBuffer, 0, 8)}");
            Console.WriteLine($"   Last 8 bytes:  {GetBytesAsString(imageBuffer, imageBuffer.Length - 8, 8)}\n");

            Console.WriteLine($"Total allocator usage: {allocator.TotalAllocatedBytes / (1024 * 1024)} MB");
            Console.WriteLine("No GC pressure was generated during processing!");
        }

        static void FillWithTestPattern(UnmanagedBuffer<byte> buffer, int width, int height, int channels)
        {
            Span<byte> pixels = buffer;
            
            // Create a gradient pattern
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = (y * width + x) * channels;
                    
                    // Red channel: horizontal gradient
                    pixels[pixelIndex] = (byte)((x * 255) / width);
                    
                    // Green channel: vertical gradient
                    pixels[pixelIndex + 1] = (byte)((y * 255) / height);
                    
                    // Blue channel: diagonal gradient
                    pixels[pixelIndex + 2] = (byte)(((x + y) * 255) / (width + height));
                    
                    // Alpha channel: full opacity
                    pixels[pixelIndex + 3] = 255;
                }
            }
        }

        static void AdjustBrightness(UnmanagedBuffer<byte> buffer, float factor)
        {
            Span<byte> pixels = buffer;
            
            for (int i = 0; i < pixels.Length; i++)
            {
                // Skip alpha channel (every 4th byte)
                if ((i % 4) != 3)
                {
                    int newValue = (int)(pixels[i] * factor);
                    pixels[i] = (byte)Math.Min(255, Math.Max(0, newValue));
                }
            }
        }

        static void AdjustContrast(UnmanagedBuffer<byte> buffer, float factor)
        {
            Span<byte> pixels = buffer;
            
            for (int i = 0; i < pixels.Length; i++)
            {
                // Skip alpha channel (every 4th byte)
                if ((i % 4) != 3)
                {
                    // Contrast adjustment formula
                    float value = pixels[i] / 255.0f;
                    float adjusted = (((value - 0.5f) * factor) + 0.5f) * 255.0f;
                    pixels[i] = (byte)Math.Min(255, Math.Max(0, (int)adjusted));
                }
            }
        }

        static void ConvertToGrayscale(UnmanagedBuffer<byte> buffer, int width, int height)
        {
            Span<byte> pixels = buffer;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = (y * width + x) * 4;
                    
                    // Get RGB values
                    byte r = pixels[pixelIndex];
                    byte g = pixels[pixelIndex + 1];
                    byte b = pixels[pixelIndex + 2];
                    
                    // Convert to grayscale using luminance formula
                    byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                    
                    // Set all channels to grayscale value (except alpha)
                    pixels[pixelIndex] = gray;     // Red
                    pixels[pixelIndex + 1] = gray; // Green
                    pixels[pixelIndex + 2] = gray; // Blue
                    // Alpha (pixels[pixelIndex + 3]) remains unchanged
                }
            }
        }

        static string GetBytesAsString(UnmanagedBuffer<byte> buffer, int startIndex, int count)
        {
            Span<byte> span = buffer;
            var result = "";
            for (int i = 0; i < count && (startIndex + i) < span.Length; i++)
            {
                result += $"{span[startIndex + i]:X2} ";
            }
            return result.Trim();
        }
    }
}