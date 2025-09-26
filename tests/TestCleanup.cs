using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    /// <summary>
    /// Utility class to help with defensive cleanup during test teardown.
    /// This helps prevent crashes when the test host is shutting down.
    /// </summary>
    public static class TestCleanup
    {
        private static readonly ConcurrentBag<WeakReference<IDisposable>> _disposables = new();
        private static int _cleanupInProgress = 0;

        /// <summary>
        /// Gets a value indicating whether the current platform is ARM64.
        /// </summary>
        public static bool IsArm64 => RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

        /// <summary>
        /// Tracks memory allocations for leak detection.
        /// </summary>
        private static readonly ConcurrentBag<IntPtr> _trackedAllocations = new();

        /// <summary>
        /// Registers a memory allocation for leak detection.
        /// </summary>
        public static void RegisterAllocation(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
            {
                _trackedAllocations.Add(pointer);
            }
        }

        /// <summary>
        /// Unregisters a memory allocation.
        /// </summary>
        public static void UnregisterAllocation(IntPtr pointer)
        {
            // Remove from tracked allocations (this is a simple implementation)
            var toRemove = _trackedAllocations.Where(p => p == pointer).ToList();
            foreach (var ptr in toRemove)
            {
                _trackedAllocations.TryTake(out _);
            }
        }

        /// <summary>
        /// Performs comprehensive cleanup including memory leak detection.
        /// </summary>
        public static void ComprehensiveCleanup()
        {
            var initialCount = _trackedAllocations.Count;

            // Perform standard cleanup
            Cleanup();

            // Check for memory leaks
            var remainingCount = _trackedAllocations.Count;
            if (remainingCount > 0)
            {
                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"Warning: {remainingCount} memory allocations may not have been freed properly.");
                #endif
            }

            // Clear tracked allocations
            _trackedAllocations.Clear();

            // Force garbage collection to help with cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Registers a disposable object for cleanup during test teardown.
        /// </summary>
        public static void Register(IDisposable disposable)
        {
            if (disposable == null)
                return;

            _disposables.Add(new WeakReference<IDisposable>(disposable));
        }

        /// <summary>
        /// Performs defensive cleanup of all registered disposables.
        /// This method is safe to call multiple times and won't throw exceptions.
        /// </summary>
        public static void Cleanup()
        {
            // Prevent multiple concurrent cleanup operations
            if (Interlocked.Exchange(ref _cleanupInProgress, 1) == 1)
                return;

            try
            {
                var disposables = new ConcurrentBag<IDisposable>();

                // Collect all valid disposables
                foreach (var weakRef in _disposables)
                {
                    if (weakRef.TryGetTarget(out var disposable))
                    {
                        disposables.Add(disposable);
                    }
                }

                // Dispose all collected disposables safely
                // Use sequential disposal on ARM64 to avoid potential concurrency issues
                if (IsArm64)
                {
                    foreach (var disposable in disposables)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            // Log but don't throw - we want to continue cleanup
                            #if DEBUG
                            System.Diagnostics.Debug.WriteLine($"Exception during test cleanup on ARM64: {ex}");
                            #endif
                        }
                    }
                }
                else
                {
                    Parallel.ForEach(disposables, disposable =>
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            // Log but don't throw - we want to continue cleanup
                            #if DEBUG
                            System.Diagnostics.Debug.WriteLine($"Exception during test cleanup: {ex}");
                            #endif
                        }
                    });
                }

                // Clear the collection
                _disposables.Clear();
            }
            catch (Exception ex)
            {
                // Log but don't throw - cleanup should never crash
                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"Exception during test cleanup operation: {ex}");
                #endif
            }
            finally
            {
                _cleanupInProgress = 0;
            }
        }

        /// <summary>
        /// Registers an allocator for cleanup during test teardown.
        /// </summary>
        public static void RegisterAllocator(IUnmanagedMemoryAllocator allocator)
        {
            if (allocator is IDisposable disposable)
            {
                Register(disposable);
            }
        }

        /// <summary>
        /// Registers a buffer for cleanup during test teardown.
        /// </summary>
        public static void RegisterBuffer<T>(UnmanagedBuffer<T> buffer) where T : unmanaged
        {
            Register(buffer);
        }
    }
}