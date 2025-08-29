using System;
using System.Collections.Generic;

namespace ZiggyAlloc
{
    /// <summary>
    /// Implements Zig's defer pattern for automatic cleanup in reverse order.
    /// </summary>
    public sealed class DeferScope : IDisposable
    {
        private readonly Stack<Action> _deferredActions = new();
        private bool _disposed = false;

        /// <summary>
        /// Starts a new defer scope.
        /// </summary>
        public static DeferScope Start() => new();

        /// <summary>
        /// Registers an action to be executed when the scope is disposed.
        /// </summary>
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
        public void Dispose()
        {
            if (_disposed)
                return;

            List<Exception>? exceptions = null;

            // Execute all deferred actions in reverse order
            while (_deferredActions.Count > 0)
            {
                try
                {
                    _deferredActions.Pop().Invoke();
                }
                catch (Exception ex)
                {
                    // Collect all exceptions
                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }

            _disposed = true;

            // Throw all exceptions if any occurred
            if (exceptions != null)
            {
                if (exceptions.Count == 1)
                    throw exceptions[0];
                else
                    throw new AggregateException(exceptions);
            }
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
        public static UnmanagedBuffer<T> AllocateDeferred<T>(
            this IUnmanagedMemoryAllocator allocator,
            DeferScope defer,
            bool zeroMemory = false) where T : unmanaged
        {
            return allocator.AllocateDeferred<T>(defer, 1, zeroMemory);
        }
    }
}