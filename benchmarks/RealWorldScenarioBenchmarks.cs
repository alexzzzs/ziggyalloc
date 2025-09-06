using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ZiggyAlloc;

namespace ZiggyAlloc.Benchmarks
{
    /// <summary>
    /// Benchmarks that simulate real-world usage scenarios for ZiggyAlloc.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class RealWorldScenarioBenchmarks
    {
        private const int ImageWidth = 1920;
        private const int ImageHeight = 1080;
        private const int ImageChannels = 4; // RGBA
        private const int ImageBufferSize = ImageWidth * ImageHeight * ImageChannels;

        private const int AudioSampleRate = 44100;
        private const int AudioDurationSeconds = 30;
        private const int AudioBufferSize = AudioSampleRate * AudioDurationSeconds;

        private const int NetworkPacketSize = 1500;
        private const int NetworkPacketCount = 10000;

        private const int DatabaseRecordCount = 100000;
        private const int DatabaseRecordSize = 128;

        private SystemMemoryAllocator _systemAllocator = null!;
        private UnmanagedMemoryPool _memoryPool = null!;
        private HybridAllocator _hybridAllocator = null!;
        private SlabAllocator _slabAllocator = null!;

        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
            _memoryPool = new UnmanagedMemoryPool(_systemAllocator);
            _hybridAllocator = new HybridAllocator(_systemAllocator);
            _slabAllocator = new SlabAllocator(_systemAllocator);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _memoryPool.Dispose();
            _hybridAllocator.Dispose();
            _slabAllocator.Dispose();
        }

        /// <summary>
        /// Simulates image processing workflow where large buffers are allocated for image data.
        /// </summary>
        [Benchmark(Baseline = true)]
        public void ManagedImageProcessing()
        {
            // Allocate image buffer
            var imageBuffer = new byte[ImageBufferSize];
            
            // Simulate image processing
            for (int i = 0; i < imageBuffer.Length; i += 4)
            {
                // RGBA manipulation
                imageBuffer[i] = 255;     // R
                imageBuffer[i + 1] = 128; // G
                imageBuffer[i + 2] = 0;   // B
                imageBuffer[i + 3] = 255; // A
            }
            
            // Simulate some processing
            for (int i = 0; i < 1000; i++)
            {
                int index = (i * 100) % imageBuffer.Length;
                imageBuffer[index] = (byte)(imageBuffer[index] * 0.9f);
            }
        }

        [Benchmark]
        public void UnmanagedImageProcessing()
        {
            using var imageBuffer = _systemAllocator.Allocate<byte>(ImageBufferSize);
            
            // Simulate image processing
            for (int i = 0; i < imageBuffer.Length; i += 4)
            {
                // RGBA manipulation
                imageBuffer[i] = 255;     // R
                imageBuffer[i + 1] = 128; // G
                imageBuffer[i + 2] = 0;   // B
                imageBuffer[i + 3] = 255; // A
            }
            
            // Simulate some processing
            for (int i = 0; i < 1000; i++)
            {
                int index = (i * 100) % imageBuffer.Length;
                imageBuffer[index] = (byte)(imageBuffer[index] * 0.9f);
            }
        }

        [Benchmark]
        public void PooledImageProcessing()
        {
            using var imageBuffer = _memoryPool.Allocate<byte>(ImageBufferSize);
            
            // Simulate image processing
            for (int i = 0; i < imageBuffer.Length; i += 4)
            {
                // RGBA manipulation
                imageBuffer[i] = 255;     // R
                imageBuffer[i + 1] = 128; // G
                imageBuffer[i + 2] = 0;   // B
                imageBuffer[i + 3] = 255; // A
            }
            
            // Simulate some processing
            for (int i = 0; i < 1000; i++)
            {
                int index = (i * 100) % imageBuffer.Length;
                imageBuffer[index] = (byte)(imageBuffer[index] * 0.9f);
            }
        }

        /// <summary>
        /// Simulates audio processing with large continuous buffers.
        /// </summary>
        [Benchmark]
        public void ManagedAudioProcessing()
        {
            // Allocate audio buffer
            var audioBuffer = new float[AudioBufferSize];
            
            // Simulate audio processing (e.g., applying gain)
            for (int i = 0; i < audioBuffer.Length; i++)
            {
                audioBuffer[i] = audioBuffer[i] * 0.8f; // Reduce volume by 20%
            }
            
            // Simulate some FFT-like processing
            for (int i = 0; i < 10000; i++)
            {
                int index1 = (i * 10) % audioBuffer.Length;
                int index2 = (i * 11) % audioBuffer.Length;
                float temp = audioBuffer[index1];
                audioBuffer[index1] = audioBuffer[index2];
                audioBuffer[index2] = temp;
            }
        }

        [Benchmark]
        public void UnmanagedAudioProcessing()
        {
            using var audioBuffer = _systemAllocator.Allocate<float>(AudioBufferSize);
            
            // Simulate audio processing (e.g., applying gain)
            for (int i = 0; i < audioBuffer.Length; i++)
            {
                audioBuffer[i] = audioBuffer[i] * 0.8f; // Reduce volume by 20%
            }
            
            // Simulate some FFT-like processing
            for (int i = 0; i < 10000; i++)
            {
                int index1 = (i * 10) % audioBuffer.Length;
                int index2 = (i * 11) % audioBuffer.Length;
                float temp = audioBuffer[index1];
                audioBuffer[index1] = audioBuffer[index2];
                audioBuffer[index2] = temp;
            }
        }

        /// <summary>
        /// Simulates network packet processing with many small allocations.
        /// </summary>
        [Benchmark]
        public void ManagedNetworkPacketProcessing()
        {
            var packets = new List<byte[]>();
            
            // Simulate receiving packets
            for (int i = 0; i < NetworkPacketCount; i++)
            {
                var packet = new byte[NetworkPacketSize];
                // Simulate filling packet with data
                for (int j = 0; j < Math.Min(100, packet.Length); j++)
                {
                    packet[j] = (byte)(i + j);
                }
                packets.Add(packet);
            }
            
            // Simulate processing packets
            foreach (var packet in packets)
            {
                // Simple processing
                for (int i = 0; i < Math.Min(10, packet.Length); i++)
                {
                    packet[i] = (byte)(packet[i] ^ 0xFF); // XOR with 0xFF
                }
            }
        }

        [Benchmark]
        public void UnmanagedNetworkPacketProcessing()
        {
            var packetPointers = new IntPtr[NetworkPacketCount];
            
            // Simulate receiving packets
            for (int i = 0; i < NetworkPacketCount; i++)
            {
                using var packet = _systemAllocator.Allocate<byte>(NetworkPacketSize);
                // Simulate filling packet with data
                for (int j = 0; j < Math.Min(100, packet.Length); j++)
                {
                    packet[j] = (byte)(i + j);
                }
                packetPointers[i] = packet.RawPointer;
                // In real scenario, we'd copy the pointer somewhere for later use
            }
            
            // Simulate processing packets
            for (int i = 0; i < NetworkPacketCount; i++)
            {
                unsafe
                {
                    var packet = new UnmanagedBuffer<byte>((byte*)packetPointers[i], NetworkPacketSize);
                    // Simple processing
                    for (int j = 0; j < Math.Min(10, packet.Length); j++)
                    {
                        packet[j] = (byte)(packet[j] ^ 0xFF); // XOR with 0xFF
                    }
                }
            }
        }

        [Benchmark]
        public void SlabAllocatedNetworkPacketProcessing()
        {
            // Simulate receiving and processing packets
            for (int i = 0; i < NetworkPacketCount; i++)
            {
                using var packet = _slabAllocator.Allocate<byte>(NetworkPacketSize);
                // Simulate filling packet with data
                for (int j = 0; j < Math.Min(100, packet.Length); j++)
                {
                    packet[j] = (byte)(i + j);
                }
                
                // Simple processing
                for (int j = 0; j < Math.Min(10, packet.Length); j++)
                {
                    packet[j] = (byte)(packet[j] ^ 0xFF); // XOR with 0xFF
                }
            }
        }

        /// <summary>
        /// Simulates database record processing with mixed allocation sizes.
        /// </summary>
        [Benchmark]
        public void ManagedDatabaseProcessing()
        {
            var records = new List<byte[]>();
            
            // Simulate reading records
            for (int i = 0; i < DatabaseRecordCount; i++)
            {
                var record = new byte[DatabaseRecordSize];
                // Simulate filling record with data
                for (int j = 0; j < record.Length; j++)
                {
                    record[j] = (byte)(i + j);
                }
                records.Add(record);
            }
            
            // Simulate processing records
            foreach (var record in records)
            {
                // Simple processing
                for (int i = 0; i < Math.Min(32, record.Length); i++)
                {
                    record[i] = (byte)(record[i] + 1);
                }
            }
        }

        [Benchmark]
        public void HybridDatabaseProcessing()
        {
            // Simulate reading and processing records
            for (int i = 0; i < DatabaseRecordCount; i++)
            {
                using var record = _hybridAllocator.Allocate<byte>(DatabaseRecordSize);
                // Simulate filling record with data
                for (int j = 0; j < record.Length; j++)
                {
                    record[j] = (byte)(i + j);
                }
                
                // Simple processing
                for (int j = 0; j < Math.Min(32, record.Length); j++)
                {
                    record[j] = (byte)(record[j] + 1);
                }
            }
        }

        /// <summary>
        /// Simulates a game engine scenario with various allocation patterns.
        /// </summary>
        [Benchmark]
        public void GameEngineSimulation()
        {
            // Simulate game loop with various allocations
            const int frameCount = 100;
            const int entitiesPerFrame = 50;
            const int particlesPerEntity = 20;
            
            for (int frame = 0; frame < frameCount; frame++)
            {
                // Allocate entities
                using var entities = _slabAllocator.Allocate<Entity>(entitiesPerFrame);
                
                // Initialize entities
                for (int i = 0; i < entities.Length; i++)
                {
                    entities[i] = new Entity
                    {
                        Id = frame * entitiesPerFrame + i,
                        Position = new Vector3 { X = i, Y = i * 2, Z = i * 3 },
                        Velocity = new Vector3 { X = 0.1f, Y = 0.2f, Z = 0.3f }
                    };
                }
                
                // Allocate particles for each entity
                for (int i = 0; i < entities.Length; i++)
                {
                    using var particles = _slabAllocator.Allocate<Particle>(particlesPerEntity);
                    
                    // Initialize particles
                    for (int j = 0; j < particles.Length; j++)
                    {
                        particles[j] = new Particle
                        {
                            Position = entities[i].Position,
                            Velocity = new Vector3 { X = j * 0.1f, Y = j * 0.2f, Z = j * 0.3f },
                            Lifetime = 5.0f
                        };
                    }
                    
                    // Update particles
                    for (int j = 0; j < particles.Length; j++)
                    {
                        particles[j].Position.X += particles[j].Velocity.X;
                        particles[j].Position.Y += particles[j].Velocity.Y;
                        particles[j].Position.Z += particles[j].Velocity.Z;
                        particles[j].Lifetime -= 0.1f;
                    }
                }
                
                // Allocate temporary buffer for frame processing
                using var frameBuffer = _memoryPool.Allocate<byte>(1024 * 64); // 64KB temp buffer
                
                // Simulate frame processing
                for (int i = 0; i < Math.Min(1000, frameBuffer.Length); i++)
                {
                    frameBuffer[i] = (byte)(frame + i);
                }
            }
        }

        /// <summary>
        /// Simulates scientific computing with large matrix operations.
        /// </summary>
        [Benchmark]
        public void ScientificComputing()
        {
            const int matrixSize = 1000;
            const int matrixElements = matrixSize * matrixSize;
            
            // Allocate matrices
            using var matrixA = _systemAllocator.Allocate<double>(matrixElements);
            using var matrixB = _systemAllocator.Allocate<double>(matrixElements);
            using var matrixC = _systemAllocator.Allocate<double>(matrixElements);
            
            // Initialize matrices
            var random = new Random(42); // Fixed seed for reproducible results
            for (int i = 0; i < matrixA.Length; i++)
            {
                matrixA[i] = random.NextDouble();
                matrixB[i] = random.NextDouble();
            }
            
            // Matrix multiplication (simplified)
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < matrixSize; k++)
                    {
                        sum += matrixA[i * matrixSize + k] * matrixB[k * matrixSize + j];
                    }
                    matrixC[i * matrixSize + j] = sum;
                }
            }
            
            // Simple reduction operation
            double total = 0;
            for (int i = 0; i < matrixC.Length; i++)
            {
                total += matrixC[i];
            }
        }

        public struct Entity
        {
            public int Id;
            public Vector3 Position;
            public Vector3 Velocity;
        }

        public struct Particle
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public float Lifetime;
        }

        public struct Vector3
        {
            public float X;
            public float Y;
            public float Z;
        }
    }
}