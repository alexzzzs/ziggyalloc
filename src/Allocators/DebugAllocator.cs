using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ZiggyAlloc
{
    /// <summary>
    /// Defines how memory leaks should be reported when detected.
    /// </summary>
    public enum MemoryLeakReportingMode 
    { 
        /// <summary>Log leak information to Console.Error</summary>
        Log, 
        /// <summary>Throw an InvalidOperationException with leak details</summary>
        Throw, 
        /// <summary>Break into the debugger when leaks are detected</summary>
        Break 
    }

    /// <summary>
    /// A debug memory allocator that tracks allocations and detects memory leaks.
    /// </summary>
    /// <remarks>
    /// This allocator wraps another allocator and tracks all allocations with caller information.
    /// When disposed, it reports any memory that wasn't explicitly freed, helping to identify
    /// memory leaks during development and testing.
    /// 
    /// This allocator is thread-safe and can be used from multiple threads simultaneously.
    /// The caller information (file, line, method) is captured automatically using compiler services.
    /// </remarks>
    public sealed class DebugMemoryAllocator : IMemoryAllocator, IDisposable
    {
        /// <summary>
        /// Metadata about a memory allocation for leak tracking.
        /// </summary>
        private readonly record struct AllocationMetadata(
            int SizeInBytes, 
            string SourceFilePath, 
            int SourceLineNumber, 
            string CallerMemberName);

        private readonly Dictionary<IntPtr, AllocationMetadata> _trackedAllocations = new();
        private readonly IMemoryAllocator _backingAllocator;
        private readonly string _allocatorName;
        private readonly MemoryLeakReportingMode _leakReportingMode;
        private readonly object _lockObject = new();

        /// <summary>
        /// Initializes a new debug memory allocator.
        /// </summary>
        /// <param name="name">A descriptive name for this allocator instance (used in leak reports)</param>
        /// <param name="backingAllocator">The underlying allocator to delegate actual memory operations to</param>
        /// <param name="reportingMode">How to report memory leaks when detected</param>
        /// <exception cref="ArgumentNullException">Thrown when name or backingAllocator is null</exception>
        /// <exception cref="ArgumentException">Thrown when name is empty or whitespace</exception>
        public DebugMemoryAllocator(string name, IMemoryAllocator backingAllocator, 
            MemoryLeakReportingMode reportingMode = MemoryLeakReportingMode.Log)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Allocator name cannot be null or empty", nameof(name));
            
            _allocatorName = name;
            _backingAllocator = backingAllocator ?? throw new ArgumentNullException(nameof(backingAllocator));
            _leakReportingMode = reportingMode;
        }

        /// <summary>
        /// Allocates memory for one or more instances of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of instances to allocate space for</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>A pointer to the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 1</exception>
        public Pointer<T> Allocate<T>(int count = 1, bool zeroed = false) where T : unmanaged
        {
            return AllocateWithCallerInfo<T>(count, zeroed);
        }

        /// <summary>
        /// Internal allocation method that captures caller information for leak tracking.
        /// </summary>
        private Pointer<T> AllocateWithCallerInfo<T>(int count = 1, bool zeroed = false,
            [CallerFilePath] string sourceFile = "",
            [CallerLineNumber] int sourceLine = 0,
            [CallerMemberName] string callerMember = "") where T : unmanaged
        {
            var pointer = _backingAllocator.Allocate<T>(count, zeroed);
            
            int sizeInBytes;
            unsafe { sizeInBytes = sizeof(T) * count; }
            
            var metadata = new AllocationMetadata(sizeInBytes, sourceFile, sourceLine, callerMember);
            
            lock (_lockObject)
            {
                _trackedAllocations.Add(pointer.Raw, metadata);
            }
            
            return pointer;
        }

        /// <summary>
        /// Frees previously allocated memory and removes it from leak tracking.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        /// <remarks>
        /// Passing IntPtr.Zero is safe and will be ignored.
        /// If the pointer was not allocated by this allocator, a warning is logged.
        /// </remarks>
        public void Free(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero) 
                return;

            bool wasTracked;
            lock (_lockObject)
            {
                wasTracked = _trackedAllocations.Remove(pointer);
            }

            if (!wasTracked)
            {
                Debug.WriteLine($"[DebugMemoryAllocator '{_allocatorName}'] Warning: " +
                              $"Attempted to free untracked pointer 0x{pointer:X}");
            }

            _backingAllocator.Free(pointer);
        }

        /// <summary>
        /// Allocates memory for a slice (array) of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of elements in the slice</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>A slice representing the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 0</exception>
        public Slice<T> AllocateSlice<T>(int count, bool zeroed = false) where T : unmanaged
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative");

            if (count == 0)
                return new Slice<T>(new Pointer<T>(IntPtr.Zero), 0);

            return new Slice<T>(AllocateWithCallerInfo<T>(count, zeroed), count);
        }

        /// <summary>
        /// Reports any memory leaks (unfreed allocations) and disposes the allocator.
        /// </summary>
        /// <remarks>
        /// The behavior depends on the MemoryLeakReportingMode specified during construction:
        /// - Log: Writes leak information to Console.Error
        /// - Throw: Throws InvalidOperationException with leak details
        /// - Break: Breaks into the debugger if attached
        /// </remarks>
        public void Dispose()
        {
            ReportMemoryLeaks();
        }

        /// <summary>
        /// Checks for and reports any memory leaks.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when leaks are detected and reporting mode is set to Throw
        /// </exception>
        public void ReportMemoryLeaks()
        {
            Dictionary<IntPtr, AllocationMetadata> leakedAllocations;
            
            lock (_lockObject)
            {
                if (_trackedAllocations.Count == 0)
                    return;
                
                // Create a copy to avoid holding the lock while generating the report
                leakedAllocations = new Dictionary<IntPtr, AllocationMetadata>(_trackedAllocations);
            }

            var reportBuilder = new StringBuilder()
                .AppendLine($"!!! MEMORY LEAK DETECTED in DebugMemoryAllocator '{_allocatorName}' !!!")
                .AppendLine($"Found {leakedAllocations.Count} unfreed allocation(s):")
                .AppendLine();

            foreach (var (pointer, metadata) in leakedAllocations)
            {
                reportBuilder.AppendLine($"  • Pointer: 0x{pointer:X}")
                           .AppendLine($"    Size: {metadata.SizeInBytes} bytes")
                           .AppendLine($"    Allocated at: {metadata.SourceFilePath}:{metadata.SourceLineNumber}")
                           .AppendLine($"    In method: {metadata.CallerMemberName}")
                           .AppendLine();
            }

            string leakReport = reportBuilder.ToString();

            switch (_leakReportingMode)
            {
                case MemoryLeakReportingMode.Throw:
                    throw new InvalidOperationException(leakReport);
                
                case MemoryLeakReportingMode.Break:
                    Console.Error.WriteLine(leakReport);
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    break;
                
                case MemoryLeakReportingMode.Log:
                default:
                    Console.Error.WriteLine(leakReport);
                    break;
            }
        }

        /// <summary>
        /// Gets the number of currently tracked (unfreed) allocations.
        /// </summary>
        /// <returns>The number of allocations that haven't been freed</returns>
        public int GetTrackedAllocationCount()
        {
            lock (_lockObject)
            {
                return _trackedAllocations.Count;
            }
        }
    }
}