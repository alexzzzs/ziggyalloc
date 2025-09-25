using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Runtime.CompilerServices;
using ZiggyAlloc;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class SimdPerformanceBenchmarks
{
    // Test sizes - much smaller than the large array benchmarks
    private const int SMALL_SIZE = 1024;      // 1KB
    private const int MEDIUM_SIZE = 16384;    // 16KB
    private const int LARGE_SIZE = 65536;     // 64KB

    private byte[] _testBuffer;

    [GlobalSetup]
    public void Setup()
    {
        _testBuffer = new byte[LARGE_SIZE];
        // Fill with non-zero data
        for (int i = 0; i < _testBuffer.Length; i++)
        {
            _testBuffer[i] = (byte)(i % 256);
        }
    }

    [Benchmark(Baseline = true)]
    [Arguments(SMALL_SIZE)]
    [Arguments(MEDIUM_SIZE)]
    [Arguments(LARGE_SIZE)]
    public unsafe void StandardZeroMemory(int size)
    {
        fixed (byte* ptr = _testBuffer)
        {
            // Standard byte-by-byte clearing
            for (int i = 0; i < size; i++)
            {
                ptr[i] = 0;
            }
        }
    }

    [Benchmark]
    [Arguments(SMALL_SIZE)]
    [Arguments(MEDIUM_SIZE)]
    [Arguments(LARGE_SIZE)]
    public unsafe void SimdZeroMemory(int size)
    {
        fixed (byte* ptr = _testBuffer)
        {
            SimdMemoryOperations.ZeroMemory(ptr, size);
        }
    }

    [Benchmark]
    [Arguments(SMALL_SIZE)]
    [Arguments(MEDIUM_SIZE)]
    [Arguments(LARGE_SIZE)]
    public unsafe void StandardCopyMemory(int size)
    {
        byte[] destination = new byte[size];
        fixed (byte* destPtr = destination)
        fixed (byte* srcPtr = _testBuffer)
        {
            // Standard byte-by-byte copying
            for (int i = 0; i < size; i++)
            {
                destPtr[i] = srcPtr[i];
            }
        }
    }

    [Benchmark]
    [Arguments(SMALL_SIZE)]
    [Arguments(MEDIUM_SIZE)]
    [Arguments(LARGE_SIZE)]
    public unsafe void SimdCopyMemory(int size)
    {
        byte[] destination = new byte[size];
        fixed (byte* destPtr = destination)
        fixed (byte* srcPtr = _testBuffer)
        {
            SimdMemoryOperations.CopyMemory(destPtr, srcPtr, size);
        }
    }

    [Benchmark]
    public void SimdInfo()
    {
        // Just to show SIMD support info
        var info = SimdMemoryOperations.GetSimdInfo();
    }

    [Benchmark]
    [Arguments(SMALL_SIZE)]
    [Arguments(MEDIUM_SIZE)]
    [Arguments(LARGE_SIZE)]
    public unsafe void ArrayClear(int size)
    {
        byte[] buffer = new byte[size];
        Array.Clear(buffer, 0, buffer.Length);
    }

    [Benchmark]
    [Arguments(SMALL_SIZE)]
    [Arguments(MEDIUM_SIZE)]
    [Arguments(LARGE_SIZE)]
    public unsafe void SpanClear(int size)
    {
        byte[] buffer = new byte[size];
        buffer.AsSpan().Clear();
    }

    [Benchmark]
    [Arguments(SMALL_SIZE)]
    [Arguments(MEDIUM_SIZE)]
    [Arguments(LARGE_SIZE)]
    public unsafe void BufferBlockCopy(int size)
    {
        byte[] destination = new byte[size];
        Buffer.BlockCopy(_testBuffer, 0, destination, 0, size);
    }
}