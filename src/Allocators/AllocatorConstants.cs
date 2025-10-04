using System.Runtime.CompilerServices;

namespace ZiggyAlloc
{
    /// <summary>
    /// Shared constants and utility methods for memory allocators.
    /// </summary>
    internal static class AllocatorConstants
    {
        /// <summary>
        /// Pre-defined size classes for common allocation sizes used by pooling allocators.
        /// </summary>
        public static readonly int[] SizeClasses = {
            16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 256,
            320, 384, 448, 512, 640, 768, 896, 1024, 1280, 1536, 1792, 2048, 2560, 3072, 3584, 4096
        };

        /// <summary>
        /// Maximum number of size classes supported by pooling allocators.
        /// </summary>
        public const int MaxSizeClasses = 32;

        /// <summary>
        /// Maximum number of slots per size class in pooling allocators.
        /// </summary>
        public const int MaxSlotsPerClass = 1024;

        /// <summary>
        /// Default slab size for slab allocators (1MB).
        /// </summary>
        public const int DefaultSlabSize = 1024 * 1024;

        /// <summary>
        /// Maximum allocation size that can be served by slab allocators (4KB).
        /// </summary>
        public const int MaxSlabAllocationSize = 4096;

        /// <summary>
        /// Default large block threshold (64KB).
        /// </summary>
        public const int DefaultLargeBlockThreshold = 64 * 1024;

        /// <summary>
        /// Memory alignment for large blocks (4KB).
        /// </summary>
        public const int LargeBlockAlignment = 4096;

        /// <summary>
        /// Minimum threshold for unknown types in hybrid allocator.
        /// </summary>
        public const int MinThreshold = 32;

        /// <summary>
        /// Maximum threshold for unknown types in hybrid allocator.
        /// </summary>
        public const int MaxThreshold = 1024;

        /// <summary>
        /// Finds the appropriate size class for the given size.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindSizeClass(int sizeInBytes)
        {
            for (int i = 0; i < MaxSizeClasses && i < SizeClasses.Length; i++)
            {
                if (SizeClasses[i] >= sizeInBytes)
                {
                    return i;
                }
            }
            return -1; // No suitable size class
        }
    }
}