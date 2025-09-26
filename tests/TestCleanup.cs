using System;
using System.Collections.Concurrent;
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