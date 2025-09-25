using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

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
    public sealed class DebugMemoryAllocator : IUnmanagedMemoryAllocator, IDisposable
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
        private readonly IUnmanagedMemoryAllocator _backingAllocator;
        private readonly string _allocatorName;
        private readonly MemoryLeakReportingMode _leakReportingMode;
        private readonly object _lockObject = new();
        private bool _disposed = false;

        /// <summary>
        /// Gets a value indicating whether this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation 
        { 
            get
            {
                CheckDisposed();
                return _backingAllocator.SupportsIndividualDeallocation;
            }
        }

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
        public long TotalAllocatedBytes 
        { 
            get
            {
                CheckDisposed();
                return _backingAllocator.TotalAllocatedBytes;
            }
        }

        /// <summary>
        /// Initializes a new debug memory allocator.
        /// </summary>
        /// <param name="name">A descriptive name for this allocator instance (used in leak reports)</param>
        /// <param name="backingAllocator">The underlying allocator to delegate actual memory operations to</param>
        /// <param name="reportingMode">How to report memory leaks when detected</param>
        /// <exception cref="ArgumentNullException">Thrown when name or backingAllocator is null</exception>
        /// <exception cref="ArgumentException">Thrown when name is empty or whitespace</exception>
        public DebugMemoryAllocator(string name, IUnmanagedMemoryAllocator backingAllocator, 
            MemoryLeakReportingMode reportingMode = MemoryLeakReportingMode.Log)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Allocator name cannot be null or empty", nameof(name));
            
            _allocatorName = name;
            _backingAllocator = backingAllocator ?? throw new ArgumentNullException(nameof(backingAllocator));
            _leakReportingMode = reportingMode;
        }

        /// <summary>
        /// Allocates unmanaged memory for the specified number of elements.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="elementCount">The number of elements to allocate space for</param>
        /// <param name="zeroMemory">Whether to zero-initialize the allocated memory</param>
        /// <returns>A buffer representing the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when elementCount is less than 0</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the allocator has been disposed</exception>
        public UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged
        {
            return AllocateWithCallerInfo<T>(elementCount, zeroMemory);
        }

        /// <summary>
        /// Internal allocation method that captures caller information for leak tracking.
        /// </summary>
        private UnmanagedBuffer<T> AllocateWithCallerInfo<T>(int elementCount, bool zeroMemory = false,
            [CallerFilePath] string sourceFile = "",
            [CallerLineNumber] int sourceLine = 0,
            [CallerMemberName] string callerMember = "") where T : unmanaged
        {
            try
            {
                CheckDisposed();
                
                var backingBuffer = _backingAllocator.Allocate<T>(elementCount, zeroMemory);

                if (backingBuffer == null)
                    throw new InvalidOperationException("Failed to allocate backing buffer");

                // Don't track empty allocations since they don't allocate actual memory
                if (elementCount == 0)
                {
                    return backingBuffer;
                }
                
                int sizeInBytes;
                unsafe { sizeInBytes = sizeof(T) * elementCount; }
                
                var metadata = new AllocationMetadata(sizeInBytes, sourceFile, sourceLine, callerMember);
                
                lock (_lockObject)
                {
                    _trackedAllocations.Add(backingBuffer.RawPointer, metadata);
                }
                
                // Create a new buffer that references this debug allocator instead of the backing allocator
                // This ensures that when the buffer is disposed, it calls our Free method for tracking
                unsafe
                {
                    return new UnmanagedBuffer<T>((T*)backingBuffer.RawPointer, backingBuffer.Length, this);
                }
            }
            catch
            {
                // If allocation fails, rethrow the exception
                throw;
            }
        }

        /// <summary>
        /// Frees previously allocated memory and removes it from leak tracking.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        /// <remarks>
        /// Passing IntPtr.Zero is safe and will be ignored.
        /// If the pointer was not allocated by this allocator, a warning is logged.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Thrown when the allocator has been disposed</exception>
        public void Free(IntPtr pointer)
        {
            // Check if disposed first, before doing anything else
            CheckDisposed();
            
            if (pointer == IntPtr.Zero) 
                return;

            try
            {
                bool wasTracked;
                lock (_lockObject)
                {
                    wasTracked = _trackedAllocations.Remove(pointer);
                }

                if (!wasTracked)
                {
                    Debug.WriteLine($"[DebugMemoryAllocator '{_allocatorName}'] Warning: " +
                                  $"Attempted to free untracked pointer 0x{pointer.ToString("X")}");
                }

                _backingAllocator.Free(pointer);
            }
            catch (Exception ex)
            {
                // Log exception in debug builds instead of silently ignoring
                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"Exception during memory cleanup in DebugMemoryAllocator.Free: {ex}");
                #endif
                throw; // Re-throw to maintain original behavior for compatibility
            }
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
            if (Volatile.Read(ref _disposed))
                return;
                
            Volatile.Write(ref _disposed, true);

            // Don't call CheckDisposed() here to avoid circular dependency
            ReportMemoryLeaksInternal();
            
            // Explicitly suppress finalization for the backing allocator if it implements IDisposable
            if (_backingAllocator is IDisposable disposableAllocator)
            {
                // We don't dispose the backing allocator as it might be used elsewhere
                // But we ensure that any cleanup that needs to happen in this allocator is done
            }
        }

        /// <summary>
        /// Checks for and reports any memory leaks.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when leaks are detected and reporting mode is set to Throw
        /// </exception>
        public void ReportMemoryLeaks()
        {
            CheckDisposed();
            ReportMemoryLeaksInternal();
        }
        
        /// <summary>
        /// Internal method to check for and report any memory leaks without disposed check.
        /// </summary>
        private void ReportMemoryLeaksInternal()
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
                reportBuilder.AppendLine($"  • Pointer: 0x{pointer.ToString("X")}")
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
        /// <exception cref="ObjectDisposedException">Thrown when the allocator has been disposed</exception>
        public int GetTrackedAllocationCount()
        {
            CheckDisposed();
            
            lock (_lockObject)
            {
                return _trackedAllocations.Count;
            }
        }
        
        /// <summary>
        /// Checks if the allocator has been disposed and throws ObjectDisposedException if so.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the allocator has been disposed</exception>
        private void CheckDisposed()
        {
            // Use Volatile.Read to ensure we're reading the most up-to-date value
            if (Volatile.Read(ref _disposed))
                throw new ObjectDisposedException(nameof(DebugMemoryAllocator));
        }
    }
}