using System.Threading;

namespace ZiggyAlloc
{
    /// <summary>
    /// Provides convenient static access to commonly used ZiggyAlloc components.
    /// </summary>
    public static class Z
    {
        private static SystemMemoryAllocator? _defaultAllocator;
        private static readonly object _defaultAllocatorLock = new();

        /// <summary>
        /// Gets the default system memory allocator instance.
        /// </summary>
        /// <remarks>
        /// This creates a singleton instance that is shared across the application.
        /// The instance is created lazily on first access and is not disposed automatically.
        /// Consider using dependency injection for more control over the allocator lifecycle.
        /// </remarks>
        public static SystemMemoryAllocator DefaultAllocator
        {
            get
            {
                if (_defaultAllocator == null)
                {
                    lock (_defaultAllocatorLock)
                    {
                        _defaultAllocator ??= new SystemMemoryAllocator();
                    }
                }
                return _defaultAllocator;
            }
        }

        /// <summary>
        /// Creates a new system memory allocator instance.
        /// </summary>
        /// <returns>A new SystemMemoryAllocator instance</returns>
        public static SystemMemoryAllocator CreateSystemMemoryAllocator()
        {
            return new SystemMemoryAllocator();
        }
    }
}