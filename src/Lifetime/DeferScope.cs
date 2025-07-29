using System;
using System.Collections.Generic;

namespace ZiggyAlloc
{
    /// <summary>
    /// Implements Zig's defer pattern for automatic cleanup in reverse order.
    /// </summary>
    /// <remarks>
    /// DeferScope allows you to register cleanup actions that are executed in LIFO order
    /// when the scope is disposed. This is particularly useful for resource management
    /// where you want to ensure cleanup happens in the reverse order of acquisition.
    /// 
    /// Example usage:
    /// <code>
    /// using var defer = DeferScope.Start();
    /// 
    /// var buffer1 = allocator.Allocate&lt;int&gt;(100);
    /// defer.Defer(() => buffer1.Dispose());
    /// 
    /// var buffer2 = allocator.Allocate&lt;double&gt;(50);
    /// defer.Defer(() => buffer2.Dispose());
    /// 
    /// // Cleanup happens in reverse order: buffer2, then buffer1
    /// </code>
    /// </remarks>
    public sealed class DeferScope : IDisposable
    {
        private readonly Stack<Action> _deferredActions = new();
        private bool _disposed = false;

        /// <summary>
        /// Starts a new defer scope.
        /// </summary>
        /// <returns>A new DeferScope instance</returns>
        /// <remarks>
        /// Use this with a 'using' statement to ensure proper cleanup:
        /// <code>using var defer = DeferScope.Start();</code>
        /// </remarks>
        public static DeferScope Start() => new();

        /// <summary>
        /// Registers an action to be executed when the scope is disposed.
        /// </summary>
        /// <param name="action">The cleanup action to defer</param>
        /// <exception cref="ArgumentNullException">Thrown when action is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scope has been disposed</exception>
        /// <remarks>
        /// Actions are executed in reverse order (LIFO) during disposal.
        /// This ensures resources are cleaned up in the reverse order of acquisition.
        /// </remarks>
        public void Defer(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            
            ThrowIfDisposed();
            _deferredActions.Push(action);
        }

        /// <summary>
        /// Gets the number of deferred actions currently registered.
        /// </summary>
        public int Count
        {
            get
            {
                ThrowIfDisposed();
                return _deferredActions.Count;
            }
        }

        /// <summary>
        /// Executes all deferred actions in reverse order and disposes the scope.
        /// </summary>
        /// <remarks>
        /// Actions are executed in LIFO order. If any action throws an exception,
        /// remaining actions will still be executed, but the first exception will
        /// be re-thrown after all cleanup attempts complete.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed)
                return;

            Exception? firstException = null;

            // Execute all deferred actions in reverse order
            while (_deferredActions.Count > 0)
            {
                try
                {
                    _deferredActions.Pop().Invoke();
                }
                catch (Exception ex)
                {
                    // Store first exception but continue cleanup
                    firstException ??= ex;
                }
            }

            _disposed = true;

            // Re-throw first exception if any occurred
            if (firstException != null)
                throw firstException;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DeferScope));
        }
    }

    /// <summary>
    /// Extension methods to make defer usage more convenient with allocators.
    /// </summary>
    public static class DeferExtensions
    {
        /// <summary>
        /// Allocates a buffer and automatically defers its disposal.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate</typeparam>
        /// <param name="allocator">The allocator to use</param>
        /// <param name="defer">The defer scope to register cleanup with</param>
        /// <param name="elementCount">Number of elements to allocate</param>
        /// <param name="zeroMemory">Whether to zero-initialize the memory</param>
        /// <returns>The allocated buffer (disposal is automatically deferred)</returns>
        public static UnmanagedBuffer<T> AllocateDeferred<T>(
            this IUnmanagedMemoryAllocator allocator,
            DeferScope defer,
            int elementCount,
            bool zeroMemory = false) where T : unmanaged
        {
            var buffer = allocator.Allocate<T>(elementCount, zeroMemory);
            defer.Defer(() => buffer.Dispose());
            return buffer;
        }

        /// <summary>
        /// Allocates a single element buffer and automatically defers its disposal.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate</typeparam>
        /// <param name="allocator">The allocator to use</param>
        /// <param name="defer">The defer scope to register cleanup with</param>
        /// <param name="zeroMemory">Whether to zero-initialize the memory</param>
        /// <returns>The allocated buffer (disposal is automatically deferred)</returns>
        public static UnmanagedBuffer<T> AllocateDeferred<T>(
            this IUnmanagedMemoryAllocator allocator,
            DeferScope defer,
            bool zeroMemory = false) where T : unmanaged
        {
            return allocator.AllocateDeferred<T>(defer, 1, zeroMemory);
        }
    }
}