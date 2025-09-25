using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Runtime.InteropServices;
using ZiggyAlloc;

[SimpleJob(RuntimeMoniker.Net90, baseline: true)]
[MemoryDiagnoser]
public class LargeArrayOptimizationBenchmarks
{
    private const int LARGE_ARRAY_SIZE = 1_000_000; // 1 million elements
    private const int MEDIUM_ARRAY_SIZE = 100_000;  // 100k elements
    private const int SMALL_ARRAY_SIZE = 1_000;     // 1k elements

    [Benchmark(Baseline = true)]
    [Arguments(SMALL_ARRAY_SIZE)]
    [Arguments(MEDIUM_ARRAY_SIZE)]
    [Arguments(LARGE_ARRAY_SIZE)]
    public unsafe void ManagedArray_Large(int size)
    {
        var array = new int[size];
        // Use the array to prevent optimization
        array[size / 2] = 42;
    }

    [Benchmark]
    [Arguments(SMALL_ARRAY_SIZE)]
    [Arguments(MEDIUM_ARRAY_SIZE)]
    [Arguments(LARGE_ARRAY_SIZE)]
    public unsafe void UnmanagedArray_Large(int size)
    {
        var pointer = (int*)NativeMemory.Alloc((nuint)size * (nuint)sizeof(int));
        try
        {
            using var buffer = new UnmanagedBuffer<int>(pointer, size);
            // Use the buffer to prevent optimization
            buffer[size / 2] = 42;
        }
        finally
        {
            NativeMemory.Free(pointer);
        }
    }

    [Benchmark]
    [Arguments(SMALL_ARRAY_SIZE)]
    [Arguments(MEDIUM_ARRAY_SIZE)]
    [Arguments(LARGE_ARRAY_SIZE)]
    public unsafe void HybridAllocator_Large(int size)
    {
        using var allocator = new HybridAllocator(new SystemMemoryAllocator());
        using var buffer = allocator.Allocate<int>(size);
        // Use the buffer to prevent optimization
        buffer[size / 2] = 42;
    }

    [Benchmark]
    [Arguments(SMALL_ARRAY_SIZE)]
    [Arguments(MEDIUM_ARRAY_SIZE)]
    [Arguments(LARGE_ARRAY_SIZE)]
    public unsafe void LargeBlockAllocator_Direct(int size)
    {
        using var allocator = new LargeBlockAllocator(new SystemMemoryAllocator());
        using var buffer = allocator.Allocate<int>(size);
        // Use the buffer to prevent optimization
        buffer[size / 2] = 42;
    }

    [Benchmark]
    [Arguments(LARGE_ARRAY_SIZE)]
    public unsafe void ArrayPool_Large()
    {
        var array = System.Buffers.ArrayPool<int>.Shared.Rent(LARGE_ARRAY_SIZE);
        try
        {
            // Use the array to prevent optimization
            array[LARGE_ARRAY_SIZE / 2] = 42;
        }
        finally
        {
            System.Buffers.ArrayPool<int>.Shared.Return(array);
        }
    }

    [Benchmark]
    [Arguments(LARGE_ARRAY_SIZE)]
    public unsafe void NativeMemory_Large()
    {
        var sizeInBytes = LARGE_ARRAY_SIZE * sizeof(int);
        var pointer = (int*)NativeMemory.Alloc((nuint)sizeInBytes);

        try
        {
            // Use the memory to prevent optimization
            pointer[LARGE_ARRAY_SIZE / 2] = 42;
        }
        finally
        {
            NativeMemory.Free(pointer);
        }
    }

    [Benchmark]
    [Arguments(LARGE_ARRAY_SIZE)]
    public unsafe void LargeBlockAllocator_WithPooling()
    {
        using var allocator = new LargeBlockAllocator(new SystemMemoryAllocator());
        using var buffer1 = allocator.Allocate<int>(LARGE_ARRAY_SIZE);
        using var buffer2 = allocator.Allocate<int>(LARGE_ARRAY_SIZE);
        using var buffer3 = allocator.Allocate<int>(LARGE_ARRAY_SIZE);

        // Use the buffers to prevent optimization
        buffer1[LARGE_ARRAY_SIZE / 2] = 42;
        buffer2[LARGE_ARRAY_SIZE / 2] = 43;
        buffer3[LARGE_ARRAY_SIZE / 2] = 44;
    }

    [Benchmark]
    [Arguments(LARGE_ARRAY_SIZE)]
    public unsafe void LargeBlockAllocator_Reused()
    {
        using var allocator = new LargeBlockAllocator(new SystemMemoryAllocator());

        // Allocate and free multiple times to test pooling
        for (int i = 0; i < 5; i++)
        {
            using var buffer = allocator.Allocate<int>(LARGE_ARRAY_SIZE);
            buffer[LARGE_ARRAY_SIZE / 2] = 42 + i;
        }
    }
}