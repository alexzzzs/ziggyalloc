namespace ZiggyAlloc
{
    /// <summary>
    /// Provides convenient static access to commonly used ZiggyAlloc components.
    /// </summary>
    public static class Z
    {
        /// <summary>
        /// Gets the default system memory allocator instance.
        /// </summary>
        public static readonly SystemMemoryAllocator DefaultAllocator = new();
    }
}