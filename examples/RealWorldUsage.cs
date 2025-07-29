using System;
using System.Runtime.InteropServices;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples
{
    /// <summary>
    /// Demonstrates real-world scenarios where ZiggyAlloc provides significant value
    /// over standard .NET memory management.
    /// </summary>
    public static class RealWorldUsage
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== ZiggyAlloc Real-World Usage Examples ===\n");

            // Example 1: Native API Interop
            DemonstrateNativeInterop();
            
            // Example 2: Large Buffer Management (avoiding GC pressure)
            DemonstrateLargeBufferManagement();
            
            // Example 3: High-Performance Image Processing
            DemonstrateImageProcessing();
            
            // Example 4: Scientific Computing with Large Datasets
            DemonstrateScientificComputing();
        }

        /// <summary>
        /// Shows how ZiggyAlloc simplifies interop with native APIs that require contiguous memory.
        /// </summary>
        static void DemonstrateNativeInterop()
        {
            Console.WriteLine("1. Native API Interop");
            Console.WriteLine("--------------------");

            var allocator = new SystemMemoryAllocator();

            // Allocate memory for native API call
            using var buffer = allocator.Allocate<Point3D>(1000, zeroMemory: true);
            
            // Fill with test data
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new Point3D 
                { 
                    X = i * 0.1f, 
                    Y = i * 0.2f, 
                    Z = i * 0.3f 
                };
            }

            // Pass directly to native API (simulated)
            ProcessPointsNative(buffer.RawPointer, buffer.Length);
            
            Console.WriteLine($"✓ Processed {buffer.Length} 3D points via native API");
            Console.WriteLine($"  Memory used: {buffer.SizeInBytes:N0} bytes");
            Console.WriteLine($"  Raw pointer: 0x{buffer.RawPointer:X}\n");
        }

        /// <summary>
        /// Shows how ZiggyAlloc helps manage large buffers without GC pressure.
        /// </summary>
        static void DemonstrateLargeBufferManagement()
        {
            Console.WriteLine("2. Large Buffer Management (No GC Pressure)");
            Console.WriteLine("-------------------------------------------");

            var allocator = new SystemMemoryAllocator();
            
            // Allocate a large buffer (100MB) - this would cause GC pressure if managed
            const int bufferSize = 100 * 1024 * 1024 / sizeof(float); // 100MB of floats
            using var largeBuffer = allocator.Allocate<float>(bufferSize, zeroMemory: false);
            
            Console.WriteLine($"✓ Allocated {largeBuffer.SizeInBytes / (1024 * 1024)}MB buffer");
            Console.WriteLine($"  Elements: {largeBuffer.Length:N0}");
            Console.WriteLine($"  Allocator total: {allocator.TotalAllocatedBytes / (1024 * 1024)}MB");

            // Use Span<T> for high-performance operations
            Span<float> span = largeBuffer;
            
            // Fill with data using vectorized operations
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = MathF.Sin(i * 0.001f);
            }

            // Process data in chunks to demonstrate real-world usage
            ProcessLargeDataset(span);
            
            Console.WriteLine("✓ Processed large dataset without GC allocations\n");
        }

        /// <summary>
        /// Shows high-performance image processing using unmanaged buffers.
        /// </summary>
        static void DemonstrateImageProcessing()
        {
            Console.WriteLine("3. High-Performance Image Processing");
            Console.WriteLine("-----------------------------------");

            var allocator = new SystemMemoryAllocator();
            
            // Simulate a 4K image (3840x2160 RGBA pixels)
            const int width = 3840;
            const int height = 2160;
            const int channels = 4; // RGBA
            const int pixelCount = width * height * channels;

            using var imageBuffer = allocator.Allocate<byte>(pixelCount, zeroMemory: false);
            using var tempBuffer = allocator.Allocate<float>(width * height, zeroMemory: false);

            Console.WriteLine($"✓ Allocated 4K image buffers:");
            Console.WriteLine($"  Image: {imageBuffer.SizeInBytes / (1024 * 1024)}MB");
            Console.WriteLine($"  Temp: {tempBuffer.SizeInBytes / (1024 * 1024)}MB");

            // Fill with test pattern
            FillImageWithTestPattern(imageBuffer, width, height);

            // Apply image processing operations
            ApplyGaussianBlur(imageBuffer, tempBuffer, width, height);

            Console.WriteLine("✓ Applied Gaussian blur to 4K image");
            Console.WriteLine($"  Total memory: {allocator.TotalAllocatedBytes / (1024 * 1024)}MB\n");
        }

        /// <summary>
        /// Shows scientific computing with large datasets and custom memory layouts.
        /// </summary>
        static void DemonstrateScientificComputing()
        {
            Console.WriteLine("4. Scientific Computing - Struct of Arrays");
            Console.WriteLine("------------------------------------------");

            var allocator = new SystemMemoryAllocator();
            const int particleCount = 1_000_000;

            // Struct-of-Arrays layout for better cache performance
            using var positions = allocator.Allocate<Vector3>(particleCount, zeroMemory: true);
            using var velocities = allocator.Allocate<Vector3>(particleCount, zeroMemory: true);
            using var masses = allocator.Allocate<float>(particleCount, zeroMemory: false);

            Console.WriteLine($"✓ Allocated particle simulation data:");
            Console.WriteLine($"  Particles: {particleCount:N0}");
            Console.WriteLine($"  Memory: {allocator.TotalAllocatedBytes / (1024 * 1024)}MB");

            // Initialize particle data
            InitializeParticles(positions, velocities, masses);

            // Run physics simulation step
            var deltaTime = 1.0f / 60.0f; // 60 FPS
            UpdateParticlePhysics(positions, velocities, masses, deltaTime);

            Console.WriteLine("✓ Completed physics simulation step");
            Console.WriteLine($"  Cache-friendly struct-of-arrays layout");
            Console.WriteLine($"  No GC pressure from large datasets\n");
        }

        // Helper methods and native API simulation

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualLock(IntPtr lpAddress, UIntPtr dwSize);

        static void ProcessPointsNative(IntPtr points, int count)
        {
            // Simulate native API call
            // In real code, this would be a P/Invoke to a native library
            Console.WriteLine($"  [Native] Processing {count} points at 0x{points:X}");
        }

        static void ProcessLargeDataset(Span<float> data)
        {
            // Process in 1MB chunks to simulate real-world streaming
            const int chunkSize = 1024 * 1024 / sizeof(float);
            int chunksProcessed = 0;

            for (int i = 0; i < data.Length; i += chunkSize)
            {
                int currentChunkSize = Math.Min(chunkSize, data.Length - i);
                var chunk = data.Slice(i, currentChunkSize);
                
                // Simulate processing (find min/max)
                float min = float.MaxValue, max = float.MinValue;
                foreach (var value in chunk)
                {
                    if (value < min) min = value;
                    if (value > max) max = value;
                }
                
                chunksProcessed++;
            }

            Console.WriteLine($"  Processed {chunksProcessed} chunks of data");
        }

        static void FillImageWithTestPattern(UnmanagedBuffer<byte> image, int width, int height)
        {
            Span<byte> pixels = image;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = (y * width + x) * 4;
                    pixels[pixelIndex + 0] = (byte)(x * 255 / width);     // R
                    pixels[pixelIndex + 1] = (byte)(y * 255 / height);    // G
                    pixels[pixelIndex + 2] = (byte)((x + y) * 255 / (width + height)); // B
                    pixels[pixelIndex + 3] = 255; // A
                }
            }
        }

        static void ApplyGaussianBlur(UnmanagedBuffer<byte> image, UnmanagedBuffer<float> temp, int width, int height)
        {
            // Simplified blur implementation for demonstration
            Span<byte> imageSpan = image;
            Span<float> tempSpan = temp;

            // Convert to float and apply simple box blur
            for (int i = 0; i < width * height; i++)
            {
                int pixelIndex = i * 4;
                float gray = (imageSpan[pixelIndex] + imageSpan[pixelIndex + 1] + imageSpan[pixelIndex + 2]) / 3.0f;
                tempSpan[i] = gray;
            }

            // Apply blur kernel (simplified)
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int index = y * width + x;
                    float blurred = (tempSpan[index - width - 1] + tempSpan[index - width] + tempSpan[index - width + 1] +
                                   tempSpan[index - 1] + tempSpan[index] + tempSpan[index + 1] +
                                   tempSpan[index + width - 1] + tempSpan[index + width] + tempSpan[index + width + 1]) / 9.0f;
                    
                    int pixelIndex = index * 4;
                    byte blurredByte = (byte)Math.Clamp(blurred, 0, 255);
                    imageSpan[pixelIndex] = blurredByte;
                    imageSpan[pixelIndex + 1] = blurredByte;
                    imageSpan[pixelIndex + 2] = blurredByte;
                }
            }
        }

        static void InitializeParticles(UnmanagedBuffer<Vector3> positions, UnmanagedBuffer<Vector3> velocities, UnmanagedBuffer<float> masses)
        {
            var random = new Random(42);
            
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = new Vector3(
                    (float)(random.NextDouble() * 1000 - 500),
                    (float)(random.NextDouble() * 1000 - 500),
                    (float)(random.NextDouble() * 1000 - 500)
                );
                
                velocities[i] = new Vector3(
                    (float)(random.NextDouble() * 10 - 5),
                    (float)(random.NextDouble() * 10 - 5),
                    (float)(random.NextDouble() * 10 - 5)
                );
                
                masses[i] = (float)(random.NextDouble() * 10 + 1);
            }
        }

        static void UpdateParticlePhysics(UnmanagedBuffer<Vector3> positions, UnmanagedBuffer<Vector3> velocities, UnmanagedBuffer<float> masses, float deltaTime)
        {
            // Simple physics update - in real code this would be much more complex
            for (int i = 0; i < positions.Length; i++)
            {
                // Apply gravity
                velocities[i] = new Vector3(
                    velocities[i].X,
                    velocities[i].Y - 9.81f * deltaTime,
                    velocities[i].Z
                );
                
                // Update position
                positions[i] = new Vector3(
                    positions[i].X + velocities[i].X * deltaTime,
                    positions[i].Y + velocities[i].Y * deltaTime,
                    positions[i].Z + velocities[i].Z * deltaTime
                );
            }
        }
    }

    // Supporting data structures
    [StructLayout(LayoutKind.Sequential)]
    public struct Point3D
    {
        public float X, Y, Z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public float X, Y, Z;

        public Vector3(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
    }
}