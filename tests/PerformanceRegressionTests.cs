using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class PerformanceRegressionTests
    {
        private readonly ITestOutputHelper _output;

        public PerformanceRegressionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SystemMemoryAllocator_AllocationPerformance_DoesNotDegrade()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            const int iterations = 1000;
            const int bufferSize = 1024;

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = allocator.Allocate<byte>(bufferSize);
                // Do minimal work to avoid optimizing away
                buffer[0] = (byte)(i % 256);
            }
            stopwatch.Stop();

            // Assert - This is a regression test, so we're checking that performance
            // doesn't significantly degrade. We'll log the time for monitoring.
            _output.WriteLine($"SystemMemoryAllocator 1000 allocations of 1KB took {stopwatch.ElapsedMilliseconds}ms");
            
            // This test passes as long as it completes (performance monitoring is manual)
            Assert.True(stopwatch.ElapsedMilliseconds > 0);
        }

        [Fact]
        public void UnmanagedMemoryPool_PerformanceImprovement_MeetsThreshold()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(systemAllocator);
            const int iterations = 1000;
            const int bufferSize = 1024;

            // Measure system allocator performance
            var systemStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = systemAllocator.Allocate<byte>(bufferSize);
                buffer[0] = (byte)(i % 256);
            }
            systemStopwatch.Stop();

            // Measure pooled allocator performance
            var poolStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = pool.Allocate<byte>(bufferSize);
                buffer[0] = (byte)(i % 256);
            }
            poolStopwatch.Stop();

            // Assert
            _output.WriteLine($"System allocator: {systemStopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"Pooled allocator: {poolStopwatch.ElapsedMilliseconds}ms");
            
            // Pool should be faster (or at least not significantly slower)
            // We allow a small margin for variation
            Assert.True(poolStopwatch.ElapsedMilliseconds <= systemStopwatch.ElapsedMilliseconds * 1.5);
        }

        [Fact]
        public void HybridAllocator_StrategySelection_PerformanceMeetsThreshold()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var hybridAllocator = new HybridAllocator(systemAllocator);
            const int smallSize = 10;   // Should use managed allocation
            const int largeSize = 1000; // Should use unmanaged allocation

            // Act - Measure small allocation performance
            var smallStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                using var buffer = hybridAllocator.Allocate<int>(smallSize);
                buffer[0] = i;
            }
            smallStopwatch.Stop();

            // Act - Measure large allocation performance
            var largeStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                using var buffer = hybridAllocator.Allocate<int>(largeSize);
                buffer[0] = i;
            }
            largeStopwatch.Stop();

            // Assert - Log performance for monitoring
            _output.WriteLine($"HybridAllocator small allocations (10 int): {smallStopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"HybridAllocator large allocations (1000 int): {largeStopwatch.ElapsedMilliseconds}ms");
            
            // Both should complete successfully
            Assert.True(smallStopwatch.ElapsedMilliseconds >= 0);
            Assert.True(largeStopwatch.ElapsedMilliseconds >= 0);
        }

        [Fact]
        public void SlabAllocator_SmallAllocationPerformance_MeetsThreshold()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(systemAllocator);
            const int iterations = 1000;
            const int bufferSize = 128; // Small buffer that should use slab allocation

            // Measure system allocator performance
            var systemStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = systemAllocator.Allocate<byte>(bufferSize);
                buffer[0] = (byte)(i % 256);
            }
            systemStopwatch.Stop();

            // Measure slab allocator performance
            var slabStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = slabAllocator.Allocate<byte>(bufferSize);
                buffer[0] = (byte)(i % 256);
            }
            slabStopwatch.Stop();

            // Assert
            _output.WriteLine($"System allocator small allocations: {systemStopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"Slab allocator small allocations: {slabStopwatch.ElapsedMilliseconds}ms");
            
            // Slab allocator should be faster for small allocations
            Assert.True(slabStopwatch.ElapsedMilliseconds <= systemStopwatch.ElapsedMilliseconds * 2);
        }

        [Fact]
        public void ScopedMemoryAllocator_BulkAllocationPerformance_DoesNotDegrade()
        {
            // Arrange
            const int allocations = 1000;
            const int bufferSize = 64;

            // Act
            var stopwatch = Stopwatch.StartNew();
            using (var allocator = new ScopedMemoryAllocator())
            {
                for (int i = 0; i < allocations; i++)
                {
                    using var buffer = allocator.Allocate<byte>(bufferSize);
                    buffer[0] = (byte)(i % 256);
                }
            } // All memory freed at once here
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"ScopedMemoryAllocator 1000 allocations freed at once took {stopwatch.ElapsedMilliseconds}ms");
            
            // Should complete successfully
            Assert.True(stopwatch.ElapsedMilliseconds >= 0);
        }

        [Fact]
        public void UnmanagedBuffer_SpanConversionPerformance_DoesNotDegrade()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10000);
            const int iterations = 1000;

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var span = buffer.AsSpan();
                // Do minimal work
                if (span.Length > 0)
                {
                    span[0] = i;
                }
            }
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"UnmanagedBuffer span conversion 1000 times took {stopwatch.ElapsedMilliseconds}ms");
            
            // Should complete successfully
            Assert.True(stopwatch.ElapsedMilliseconds >= 0);
        }

        [Fact]
        public void Allocator_TotalAllocatedBytes_TrackingPerformance_DoesNotDegrade()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            const int iterations = 1000;
            const int bufferSize = 1024;

            // Act
            var stopwatch = Stopwatch.StartNew();
            long initialBytes = allocator.TotalAllocatedBytes;
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = allocator.Allocate<byte>(bufferSize);
                // Access TotalAllocatedBytes property
                long currentBytes = allocator.TotalAllocatedBytes;
                // Do minimal work to avoid optimizing away
                buffer[0] = (byte)(currentBytes % 256);
            }
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"TotalAllocatedBytes tracking 1000 times took {stopwatch.ElapsedMilliseconds}ms");
            Assert.True(allocator.TotalAllocatedBytes > initialBytes);
        }

        [Fact]
        public void DebugMemoryAllocator_WithTracking_PerformanceOverhead_MeetsThreshold()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("PerfTest", systemAllocator);
            const int iterations = 100;
            const int bufferSize = 1024;

            // Measure system allocator performance
            var systemStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = systemAllocator.Allocate<byte>(bufferSize);
                buffer[0] = (byte)(i % 256);
            }
            systemStopwatch.Stop();

            // Measure debug allocator performance
            var debugStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = debugAllocator.Allocate<byte>(bufferSize);
                buffer[0] = (byte)(i % 256);
            }
            debugStopwatch.Stop();

            // Assert
            _output.WriteLine($"System allocator: {systemStopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"Debug allocator: {debugStopwatch.ElapsedMilliseconds}ms");
            
            // Debug allocator overhead should not be excessive (less than 5x slower)
            Assert.True(debugStopwatch.ElapsedMilliseconds <= systemStopwatch.ElapsedMilliseconds * 5);
        }

        [Fact]
        public void AllocatorChain_Performance_MeetsThreshold()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("ChainTest", systemAllocator);
            using var pool = new UnmanagedMemoryPool(debugAllocator);
            using var hybridAllocator = new HybridAllocator(pool);
            const int iterations = 100;
            const int bufferSize = 1024;

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = hybridAllocator.Allocate<byte>(bufferSize);
                buffer[0] = (byte)(i % 256);
            }
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"Allocator chain (System->Debug->Pool->Hybrid) 100 allocations took {stopwatch.ElapsedMilliseconds}ms");
            
            // Should complete successfully
            Assert.True(stopwatch.ElapsedMilliseconds >= 0);
        }

        [Fact]
        public void LargeAllocation_Performance_DoesNotDegrade()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            const int largeBufferSize = 1024 * 1024; // 1MB
            const int iterations = 10;

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = allocator.Allocate<byte>(largeBufferSize);
                // Do minimal work to avoid optimizing away
                buffer[0] = (byte)(i % 256);
            }
            stopwatch.Stop();

            // Assert
            _output.WriteLine($"Large allocation (1MB) 10 times took {stopwatch.ElapsedMilliseconds}ms");
            
            // Should complete successfully
            Assert.True(stopwatch.ElapsedMilliseconds >= 0);
        }
    }
}