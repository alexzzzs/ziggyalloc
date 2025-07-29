namespace ZiggyAlloc
{
    /// <summary>
    /// Provides convenient static access to commonly used ZiggyAlloc components.
    /// </summary>
    /// <remarks>
    /// This class offers quick access to a default allocator for simple use cases.
    /// For more complex scenarios or when you need specific allocator configurations,
    /// create your own instances directly.
    /// </remarks>
    public static class Z
    {
        /// <summary>
        /// Gets the default system memory allocator instance.
        /// </summary>
        /// <remarks>
        /// This allocator uses the platform's native memory allocation functions.
        /// It's thread-safe and suitable for most general-purpose scenarios.
        /// </remarks>
        public static readonly SystemMemoryAllocator DefaultAllocator = new();
    }
}