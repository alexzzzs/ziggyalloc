using System;
using ZiggyAlloc;

/// <summary>
/// A realistic example showing how a user might integrate ZiggyAlloc
/// into their application for high-performance data processing.
/// </summary>
public class RealWorldExample
{
    public static void RunImageProcessingExample()
    {
        Console.WriteLine("\n=== Real-World Example: Image Processing ===");
        
        // Simulate processing a 1920x1080 RGBA image
        const int width = 1920;
        const int height = 1080;
        const int channels = 4; // RGBA
        const int imageSize = width * height * channels;
        
        using var allocator = new DebugAllocator("ImageProcessor", Z.DefaultAllocator);
        var ctx = new Ctx(allocator, Z.ctx.@out, Z.ctx.@in);
        
        using var defer = DeferScope.Start();
        
        // Allocate image buffers
        var inputImage = ctx.AllocSlice<byte>(defer, imageSize, zeroed: true);
        var outputImage = ctx.AllocSlice<byte>(defer, imageSize, zeroed: true);
        var tempBuffer = ctx.AllocSlice<float>(defer, width * height, zeroed: true);
        
        Console.WriteLine($"Allocated buffers for {width}x{height} image ({imageSize:N0} bytes)");
        
        // Simulate filling input with test pattern
        FillTestPattern(inputImage, width, height);
        
        // Simulate image processing operations
        ApplyGrayscaleFilter(inputImage, outputImage, tempBuffer, width, height);
        
        // Verify results
        var inputSpan = inputImage.AsSpan();
        var outputSpan = outputImage.AsSpan();
        
        Console.WriteLine($"Input pixel [0]: R={inputSpan[0]}, G={inputSpan[1]}, B={inputSpan[2]}, A={inputSpan[3]}");
        Console.WriteLine($"Output pixel [0]: R={outputSpan[0]}, G={outputSpan[1]}, B={outputSpan[2]}, A={outputSpan[3]}");
        
        Console.WriteLine("✓ Image processing completed successfully");
    }
    
    private static void FillTestPattern(Slice<byte> image, int width, int height)
    {
        var span = image.AsSpan();
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixelIndex = (y * width + x) * 4;
                
                // Create a simple gradient pattern
                span[pixelIndex + 0] = (byte)(x * 255 / width);     // R
                span[pixelIndex + 1] = (byte)(y * 255 / height);    // G
                span[pixelIndex + 2] = (byte)((x + y) * 255 / (width + height)); // B
                span[pixelIndex + 3] = 255; // A (fully opaque)
            }
        }
    }
    
    private static void ApplyGrayscaleFilter(Slice<byte> input, Slice<byte> output, 
                                           Slice<float> temp, int width, int height)
    {
        var inputSpan = input.AsSpan();
        var outputSpan = output.AsSpan();
        var tempSpan = temp.AsSpan();
        
        // Convert to grayscale using luminance formula
        for (int i = 0; i < width * height; i++)
        {
            int pixelIndex = i * 4;
            
            float r = inputSpan[pixelIndex + 0] / 255.0f;
            float g = inputSpan[pixelIndex + 1] / 255.0f;
            float b = inputSpan[pixelIndex + 2] / 255.0f;
            
            // Standard luminance formula
            float gray = 0.299f * r + 0.587f * g + 0.114f * b;
            tempSpan[i] = gray;
            
            byte grayByte = (byte)(gray * 255);
            outputSpan[pixelIndex + 0] = grayByte; // R
            outputSpan[pixelIndex + 1] = grayByte; // G
            outputSpan[pixelIndex + 2] = grayByte; // B
            outputSpan[pixelIndex + 3] = inputSpan[pixelIndex + 3]; // Keep alpha
        }
    }
}