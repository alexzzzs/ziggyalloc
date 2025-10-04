using System.Numerics;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using ZiggyAlloc;

namespace ZiggyAlloc.Benchmarks
{
    /// <summary>
    /// Benchmarks for the AlignedAllocator to measure performance improvements from memory alignment.
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [GcServer(true)]
    [GcConcurrent(true)]
    [GcForce(true)]
    public class AlignedAllocatorBenchmarks
    {
        private SystemMemoryAllocator? _systemAllocator;
        private AlignedAllocator? _alignedAllocator;
        private UnmanagedMemoryPool? _memoryPool;

        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
            _alignedAllocator = new AlignedAllocator(_systemAllocator);
            _memoryPool = new UnmanagedMemoryPool(_systemAllocator);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _alignedAllocator?.Dispose();
            _memoryPool?.Dispose();
            _systemAllocator?.Dispose();
        }

        [Benchmark(Description = "System Allocator - Small Allocations")]
        [BenchmarkCategory("Baseline")]
        public void SystemAllocator_SmallAllocations()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = _systemAllocator!.Allocate<int>(16);
                buffer[0] = i;
            }
        }

        [Benchmark(Description = "Aligned Allocator - Small Allocations")]
        [BenchmarkCategory("Aligned")]
        public void AlignedAllocator_SmallAllocations()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = _alignedAllocator!.Allocate<int>(16);
                buffer[0] = i;
            }
        }

        [Benchmark(Description = "System Allocator - SIMD Operations")]
        [BenchmarkCategory("SIMD")]
        public void SystemAllocator_SIMDOperations()
        {
            for (int i = 0; i < 100; i++)
            {
                using var buffer = _systemAllocator!.Allocate<Vector128<float>>(100);
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = Vector128.Create(i * 1.0f);
                }
            }
        }

        [Benchmark(Description = "Aligned Allocator - SIMD Operations")]
        [BenchmarkCategory("SIMD")]
        public void AlignedAllocator_SIMDOperations()
        {
            for (int i = 0; i < 100; i++)
            {
                using var buffer = _alignedAllocator!.Allocate<Vector128<float>>(100);
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = Vector128.Create(i * 1.0f);
                }
            }
        }

        [Benchmark(Description = "System Allocator - AVX Operations")]
        [BenchmarkCategory("AVX")]
        public void SystemAllocator_AVXOperations()
        {
            for (int i = 0; i < 100; i++)
            {
                using var buffer = _systemAllocator!.Allocate<Vector256<double>>(50);
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = Vector256.Create(i * 1.0);
                }
            }
        }

        [Benchmark(Description = "Aligned Allocator - AVX Operations")]
        [BenchmarkCategory("AVX")]
        public void AlignedAllocator_AVXOperations()
        {
            for (int i = 0; i < 100; i++)
            {
                using var buffer = _alignedAllocator!.Allocate<Vector256<double>>(50);
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = Vector256.Create(i * 1.0);
                }
            }
        }

        [Benchmark(Description = "System Allocator - Cache Line Operations")]
        [BenchmarkCategory("Cache")]
        public void SystemAllocator_CacheLineOperations()
        {
            for (int i = 0; i < 200; i++)
            {
                using var buffer = _systemAllocator!.Allocate<byte>(64); // Cache line size
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = (byte)(i + j);
                }
            }
        }

        [Benchmark(Description = "Aligned Allocator - Cache Line Operations")]
        [BenchmarkCategory("Cache")]
        public void AlignedAllocator_CacheLineOperations()
        {
            for (int i = 0; i < 200; i++)
            {
                using var buffer = _alignedAllocator!.Allocate<byte>(64); // Cache line size
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = (byte)(i + j);
                }
            }
        }

        [Benchmark(Description = "System Allocator - Large Buffer Operations")]
        [BenchmarkCategory("LargeBuffers")]
        public void SystemAllocator_LargeBufferOperations()
        {
            for (int i = 0; i < 50; i++)
            {
                using var buffer = _systemAllocator!.Allocate<double>(8192); // 64KB
                // Simulate cache-friendly access pattern
                for (int j = 0; j < buffer.Length; j += 8) // Step by cache line
                {
                    buffer[j] = i * 3.14159;
                }
            }
        }

        [Benchmark(Description = "Aligned Allocator - Large Buffer Operations")]
        [BenchmarkCategory("LargeBuffers")]
        public void AlignedAllocator_LargeBufferOperations()
        {
            for (int i = 0; i < 50; i++)
            {
                using var buffer = _alignedAllocator!.Allocate<double>(8192); // 64KB
                // Simulate cache-friendly access pattern
                for (int j = 0; j < buffer.Length; j += 8) // Step by cache line
                {
                    buffer[j] = i * 3.14159;
                }
            }
        }

        [Benchmark(Description = "System Allocator - Struct Array Operations")]
        [BenchmarkCategory("Structs")]
        public void SystemAllocator_StructArrayOperations()
        {
            for (int i = 0; i < 100; i++)
            {
                using var buffer = _systemAllocator!.Allocate<Matrix4x4>(100);
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = Matrix4x4.CreateRotationX(i * 0.1f);
                }
            }
        }

        [Benchmark(Description = "Aligned Allocator - Struct Array Operations")]
        [BenchmarkCategory("Structs")]
        public void AlignedAllocator_StructArrayOperations()
        {
            for (int i = 0; i < 100; i++)
            {
                using var buffer = _alignedAllocator!.Allocate<Matrix4x4>(100);
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = Matrix4x4.CreateRotationX(i * 0.1f);
                }
            }
        }

        [Benchmark(Description = "Memory Copy Operations - Unaligned")]
        [BenchmarkCategory("CopyOperations")]
        public void UnalignedMemoryCopyOperations()
        {
            for (int i = 0; i < 100; i++)
            {
                using var source = _systemAllocator!.Allocate<float>(1024);
                using var dest = _systemAllocator.Allocate<float>(1024);

                // Fill source with data
                for (int j = 0; j < source.Length; j++)
                {
                    source[j] = i * 1.0f + j;
                }

                // Copy to destination
                for (int j = 0; j < source.Length; j++)
                {
                    dest[j] = source[j];
                }
            }
        }

        [Benchmark(Description = "Memory Copy Operations - Aligned")]
        [BenchmarkCategory("CopyOperations")]
        public void AlignedMemoryCopyOperations()
        {
            for (int i = 0; i < 100; i++)
            {
                using var source = _alignedAllocator!.Allocate<float>(1024);
                using var dest = _alignedAllocator.Allocate<float>(1024);

                // Fill source with data
                for (int j = 0; j < source.Length; j++)
                {
                    source[j] = i * 1.0f + j;
                }

                // Copy to destination
                for (int j = 0; j < source.Length; j++)
                {
                    dest[j] = source[j];
                }
            }
        }

        [Benchmark(Description = "Bulk Memory Operations - Unaligned")]
        [BenchmarkCategory("BulkOperations")]
        public void UnalignedBulkMemoryOperations()
        {
            for (int i = 0; i < 50; i++)
            {
                using var buffer = _systemAllocator!.Allocate<double>(4096);

                // Bulk operations that benefit from alignment
                var span = new Span<double>((void*)buffer.RawPointer, buffer.Length);

                // Fill with pattern
                for (int j = 0; j < span.Length; j++)
                {
                    span[j] = Math.Sin(i) * Math.Cos(j);
                }

                // Calculate sum
                double sum = 0;
                for (int j = 0; j < span.Length; j++)
                {
                    sum += span[j];
                }

                // Verify sum is reasonable
                Assert.True(double.IsFinite(sum));
            }
        }

        [Benchmark(Description = "Bulk Memory Operations - Aligned")]
        [BenchmarkCategory("BulkOperations")]
        public void AlignedBulkMemoryOperations()
        {
            for (int i = 0; i < 50; i++)
            {
                using var buffer = _alignedAllocator!.Allocate<double>(4096);

                // Bulk operations that benefit from alignment
                var span = new Span<double>((void*)buffer.RawPointer, buffer.Length);

                // Fill with pattern
                for (int j = 0; j < span.Length; j++)
                {
                    span[j] = Math.Sin(i) * Math.Cos(j);
                }

                // Calculate sum
                double sum = 0;
                for (int j = 0; j < span.Length; j++)
                {
                    sum += span[j];
                }

                // Verify sum is reasonable
                Assert.True(double.IsFinite(sum));
            }
        }

        [Benchmark(Description = "Alignment Statistics Collection")]
        [BenchmarkCategory("Overhead")]
        public void AlignmentStatisticsOverhead()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = _alignedAllocator!.Allocate<int>(64);
                buffer[0] = i;

                // Collect statistics (potential overhead)
                var stats = _alignedAllocator.GetAlignmentStatistics();
            }
        }

        [Benchmark(Description = "Different Alignment Strategies")]
        [BenchmarkCategory("Strategies")]
        public void DifferentAlignmentStrategies()
        {
            var strategies = new[]
            {
                AlignmentStrategy.Natural,
                AlignmentStrategy.CacheLine,
                AlignmentStrategy.SSE,
                AlignmentStrategy.AVX,
                AlignmentStrategy.AVX512
            };

            foreach (var strategy in strategies)
            {
                using var strategyAllocator = new AlignedAllocator(_systemAllocator!, strategy);

                for (int i = 0; i < 200; i++)
                {
                    using var buffer = strategyAllocator.Allocate<float>(32);
                    buffer[0] = i;
                }
            }
        }

        [Benchmark(Description = "Mixed SIMD Type Allocations")]
        [BenchmarkCategory("MixedSIMD")]
        public void MixedSIMDTypeAllocations()
        {
            for (int i = 0; i < 100; i++)
            {
                // Mix different SIMD types that require different alignments
                using var byteBuffer = _alignedAllocator!.Allocate<Vector128<byte>>(10);
                using var intBuffer = _alignedAllocator.Allocate<Vector128<int>>(10);
                using var floatBuffer = _alignedAllocator.Allocate<Vector256<float>>(5);
                using var doubleBuffer = _alignedAllocator.Allocate<Vector256<double>>(5);

                // Initialize with data
                byteBuffer[0] = Vector128.Create((byte)i);
                intBuffer[0] = Vector128.Create(i);
                floatBuffer[0] = Vector256.Create(i * 1.0f);
                doubleBuffer[0] = Vector256.Create(i * 1.0);
            }
        }

        [Benchmark(Description = "Cache Line Aligned vs Unaligned")]
        [BenchmarkCategory("CacheAlignment")]
        public void CacheLineAlignedVsUnaligned()
        {
            const int cacheLineSize = 64;
            const int iterations = 100;

            // Unaligned allocator
            for (int i = 0; i < iterations; i++)
            {
                using var unaligned = _systemAllocator!.Allocate<byte>(cacheLineSize);
                // Access in cache-line sized chunks
                for (int j = 0; j < unaligned.Length; j += cacheLineSize)
                {
                    unaligned[j] = (byte)i;
                }
            }

            // Aligned allocator
            for (int i = 0; i < iterations; i++)
            {
                using var aligned = _alignedAllocator!.Allocate<byte>(cacheLineSize);
                // Access in cache-line sized chunks
                for (int j = 0; j < aligned.Length; j += cacheLineSize)
                {
                    aligned[j] = (byte)i;
                }
            }
        }

        [Benchmark(Description = "Memory Pool vs Aligned Allocator")]
        [BenchmarkCategory("Comparison")]
        public void MemoryPoolVsAlignedAllocator()
        {
            const int iterations = 500;

            // Memory pool
            for (int i = 0; i < iterations; i++)
            {
                using var pooled = _memoryPool!.Allocate<float>(256);
                pooled[0] = i;
            }

            // Aligned allocator
            for (int i = 0; i < iterations; i++)
            {
                using var aligned = _alignedAllocator!.Allocate<float>(256);
                aligned[0] = i;
            }
        }

        [Benchmark(Description = "Complex Data Structure Alignment")]
        [BenchmarkCategory("ComplexTypes")]
        public void ComplexDataStructureAlignment()
        {
            for (int i = 0; i < 100; i++)
            {
                // Allocate arrays of complex structs that benefit from alignment
                using var matrixBuffer = _alignedAllocator!.Allocate<Matrix4x4>(50);
                using var vectorBuffer = _alignedAllocator.Allocate<Vector3>(100);
                using var quaternionBuffer = _alignedAllocator.Allocate<Quaternion>(75);

                // Initialize with transformation data
                for (int j = 0; j < matrixBuffer.Length; j++)
                {
                    matrixBuffer[j] = Matrix4x4.CreateRotationY(i * 0.1f + j * 0.05f);
                }

                for (int j = 0; j < vectorBuffer.Length; j++)
                {
                    vectorBuffer[j] = new Vector3(i + j, i * j, j - i);
                }

                for (int j = 0; j < quaternionBuffer.Length; j++)
                {
                    quaternionBuffer[j] = Quaternion.CreateFromAxisAngle(Vector3.UnitY, i * 0.1f + j * 0.02f);
                }
            }
        }

        [Benchmark(Description = "Alignment Strategy Auto-Detection")]
        [BenchmarkCategory("AutoDetection")]
        public void AlignmentStrategyAutoDetection()
        {
            var testTypes = new Type[]
            {
                typeof(byte),
                typeof(int),
                typeof(double),
                typeof(Vector128<float>),
                typeof(Vector256<double>),
                typeof(Vector512<int>),
                typeof(Matrix4x4)
            };

            foreach (var type in testTypes)
            {
                // Test auto-detection for different types
                var method = typeof(AlignedAllocator).GetMethod(nameof(AlignedAllocator.Allocate))!
                    .MakeGenericMethod(type);

                for (int i = 0; i < 50; i++)
                {
                    // Simulate allocation with auto-detection
                    var buffer = method.Invoke(_alignedAllocator, new object[] { 100, false }) as IDisposable;
                    using (buffer) { }
                }
            }
        }
    }
}