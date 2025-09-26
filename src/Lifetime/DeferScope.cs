using System;
using System.Collections.Generic;
using System.Threading;

namespace ZiggyAlloc
{
    /// <summary>
    /// Implements Zig's defer pattern for automatic cleanup in reverse order.
    /// </summary>
    public sealed class DeferScope : IDisposable
    {
        private readonly Stack<Action> _deferredActions = new();
        private int _disposed = 0; // Use int for atomic operations instead of bool

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
            lock (_deferredActions)
            {
                _deferredActions.Push(action);
            }
        }

        /// <summary>
        /// Gets the number of deferred actions currently registered.
        /// </summary>
        public int Count
        {
            get
            {
                ThrowIfDisposed();
                lock (_deferredActions)
                {
                    return _deferredActions.Count;
                }
            }
        }

        /// <summary>
        /// Executes all deferred actions in reverse order and disposes the scope.
        /// </summary>
        public void Dispose()
        {
            // Use atomic operation to check and set disposed state
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            List<Exception>? exceptions = null;
            var actions = new List<Action>();

            // Atomically get all actions to execute (prevents race conditions)
            lock (_deferredActions)
            {
                while (_deferredActions.Count > 0)
                {
                    actions.Add(_deferredActions.Pop());
                }
            }

            // Execute all deferred actions in reverse order (outside lock for performance)
            for (int i = actions.Count - 1; i >= 0; i--)
            {
                try
                {
                    actions[i].Invoke();
                }
                catch (Exception ex)
                {
                    // Collect all exceptions
                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }

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
            if (_disposed == 1)
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
            if (allocator == null)
                throw new ArgumentNullException(nameof(allocator));
            if (defer == null)
                throw new ArgumentNullException(nameof(defer));

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
            if (allocator == null)
                throw new ArgumentNullException(nameof(allocator));
            if (defer == null)
                throw new ArgumentNullException(nameof(defer));

            return allocator.AllocateDeferred<T>(defer, 1, zeroMemory);
        }
    }
}