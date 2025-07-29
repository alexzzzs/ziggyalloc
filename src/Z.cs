namespace ZiggyAlloc
{
    /// <summary>
    /// Provides convenient static access to commonly used ZiggyAlloc components.
    /// </summary>
    /// <remarks>
    /// This class offers quick access to a default allocator and context for simple use cases.
    /// For more complex scenarios or when you need specific allocator configurations,
    /// create your own instances directly.
    /// </remarks>
    public static class ZiggyAllocDefaults
    {
        /// <summary>
        /// Gets the default manual memory allocator instance.
        /// </summary>
        /// <remarks>
        /// This allocator uses the platform's native memory allocation functions and requires
        /// explicit memory management. It's thread-safe and suitable for most general-purpose scenarios.
        /// </remarks>
        public static readonly ManualMemoryAllocator DefaultAllocator = new();

        /// <summary>
        /// Gets the default context configured with the default allocator and console I/O.
        /// </summary>
        /// <remarks>
        /// This context uses the DefaultAllocator for memory operations and console-based I/O.
        /// It's convenient for simple applications but consider creating custom contexts for
        /// production applications that need specific allocator or I/O configurations.
        /// </remarks>
        public static readonly AllocationContext DefaultContext = new(
            DefaultAllocator, 
            new ConsoleOutputWriter(), 
            new ConsoleInputReader());
    }
}