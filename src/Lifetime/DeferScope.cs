using System;
using System.Collections.Generic;

namespace ZiggyAlloc
{
    /// <summary>
    /// Manages deferred cleanup actions that are executed in reverse order when the scope is disposed.
    /// </summary>
    /// <remarks>
    /// This class implements Zig's defer pattern, where cleanup actions are registered during execution
    /// and automatically run in LIFO (Last In, First Out) order when the scope ends. This ensures
    /// that resources are cleaned up in the reverse order of their acquisition, which is typically
    /// the safest approach for nested resource management.
    /// 
    /// The deferred actions are executed even if exceptions occur, making this suitable for
    /// deterministic resource cleanup in both success and error scenarios.
    /// </remarks>
    public sealed class DeferredCleanupScope : IDisposable
    {
        private readonly Stack<Action> _deferredActions = new();
        private bool _disposed = false;

        /// <summary>
        /// Registers an action to be executed when the scope is disposed.
        /// </summary>
        /// <param name="cleanupAction">The action to execute during cleanup</param>
        /// <exception cref="ArgumentNullException">Thrown when cleanupAction is null</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the scope has already been disposed</exception>
        /// <remarks>
        /// Actions are executed in reverse order of registration (LIFO - Last In, First Out).
        /// This ensures that resources are cleaned up in the reverse order of their acquisition.
        /// </remarks>
        public void DeferAction(Action cleanupAction)
        {
            if (cleanupAction == null)
                throw new ArgumentNullException(nameof(cleanupAction));
            
            ThrowIfDisposed();
            
            _deferredActions.Push(cleanupAction);
        }

        /// <summary>
        /// Executes all deferred actions in reverse order and disposes the scope.
        /// </summary>
        /// <remarks>
        /// Actions are executed in LIFO order (Last In, First Out). If any action throws an exception,
        /// the remaining actions will still be executed, but the first exception will be re-thrown
        /// after all cleanup attempts are complete.
        /// 
        /// After disposal, no new actions can be deferred and the scope cannot be reused.
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
                    // Store the first exception but continue cleanup
                    firstException ??= ex;
                }
            }

            _disposed = true;

            // Re-throw the first exception if any occurred during cleanup
            if (firstException != null)
                throw firstException;
        }

        /// <summary>
        /// Creates and returns a new deferred cleanup scope.
        /// </summary>
        /// <returns>A new DeferredCleanupScope instance ready for use</returns>
        /// <remarks>
        /// This is a convenience method equivalent to calling 'new DeferredCleanupScope()'.
        /// The returned scope should be used with a 'using' statement to ensure proper disposal.
        /// </remarks>
        public static DeferredCleanupScope Create() => new();

        /// <summary>
        /// Gets the number of deferred actions currently registered.
        /// </summary>
        /// <returns>The number of actions that will be executed during disposal</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the scope has been disposed</exception>
        public int DeferredActionCount
        {
            get
            {
                ThrowIfDisposed();
                return _deferredActions.Count;
            }
        }

        /// <summary>
        /// Throws ObjectDisposedException if the scope has been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DeferredCleanupScope));
        }
    }
}