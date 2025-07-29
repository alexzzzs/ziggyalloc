using System;
using System.Text;

namespace ZiggyAlloc
{
    /// <summary>
    /// Represents an allocation context that combines memory allocation, input, and output operations.
    /// </summary>
    /// <remarks>
    /// This struct follows Zig's context pattern, where related functionality (memory allocation,
    /// I/O operations) is grouped together and passed through the application. This promotes
    /// explicit dependency management and makes testing easier by allowing different implementations
    /// to be injected.
    /// 
    /// The context is a readonly struct, making it lightweight and safe to pass by value.
    /// All operations delegate to the configured allocator and I/O implementations.
    /// </remarks>
    public readonly struct AllocationContext
    {
        /// <summary>
        /// The memory allocator used for all allocation operations in this context.
        /// </summary>
        public readonly IMemoryAllocator Allocator;

        /// <summary>
        /// The output writer used for all output operations in this context.
        /// </summary>
        public readonly IOutputWriter Output;

        /// <summary>
        /// The input reader used for all input operations in this context.
        /// </summary>
        public readonly IInputReader Input;

        /// <summary>
        /// Initializes a new allocation context with the specified allocator and I/O implementations.
        /// </summary>
        /// <param name="allocator">The memory allocator to use for allocation operations</param>
        /// <param name="output">The output writer to use for output operations</param>
        /// <param name="input">The input reader to use for input operations</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
        public AllocationContext(IMemoryAllocator allocator, IOutputWriter output, IInputReader input)
        {
            Allocator = allocator ?? throw new ArgumentNullException(nameof(allocator));
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Input = input ?? throw new ArgumentNullException(nameof(input));
        }

        // Output Operations

        /// <summary>
        /// Writes a string to the output without a trailing newline.
        /// </summary>
        /// <param name="text">The text to write</param>
        public void Write(string text) => Output.Write(text);

        /// <summary>
        /// Writes a single character to the output without a trailing newline.
        /// </summary>
        /// <param name="character">The character to write</param>
        public void Write(char character) => Output.Write(character);

        /// <summary>
        /// Writes a newline to the output.
        /// </summary>
        public void WriteLine() => Output.WriteLine();

        /// <summary>
        /// Writes the string representation of a value followed by a newline.
        /// </summary>
        /// <typeparam name="T">The type of the value to write</typeparam>
        /// <param name="value">The value to write</param>
        public void WriteLine<T>(T value) => Output.WriteLine(value);

        /// <summary>
        /// Writes a formatted string followed by a newline.
        /// </summary>
        /// <param name="format">The format string</param>
        /// <param name="args">The arguments to format</param>
        public void WriteLineFormatted(string format, params object[] args) => 
            Output.WriteLine(string.Format(format, args));

        // Input Operations

        /// <summary>
        /// Reads a line of text from the input source.
        /// </summary>
        /// <returns>The line of text read, or null if end of input is reached</returns>
        public string? ReadLine() => Input.ReadLine();

        /// <summary>
        /// Reads a single character from the input source.
        /// </summary>
        /// <returns>The character read, or -1 if end of input is reached</returns>
        public int ReadCharacter() => Input.Read();

        // Memory Allocation Operations

        /// <summary>
        /// Allocates memory for one or more instances of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of instances to allocate space for</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>A pointer to the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 1</exception>
        public Pointer<T> Allocate<T>(int count = 1, bool zeroed = false) where T : unmanaged =>
            Allocator.Allocate<T>(count, zeroed);

        /// <summary>
        /// Allocates memory for a slice (array) of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of elements in the slice</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>A slice representing the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 0</exception>
        public Slice<T> AllocateSlice<T>(int count, bool zeroed = false) where T : unmanaged =>
            Allocator.AllocateSlice<T>(count, zeroed);

        /// <summary>
        /// Allocates memory with automatic cleanup using RAII pattern.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of instances to allocate space for</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>An AutoFree wrapper that will automatically free the memory when disposed</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 1</exception>
        public AutoFreeMemory<T> AllocateAuto<T>(int count = 1, bool zeroed = false) where T : unmanaged =>
            new(Allocator, count, zeroed);

        // Deferred Allocation Operations

        /// <summary>
        /// Allocates memory that will be automatically freed when the defer scope is disposed.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="deferScope">The defer scope that will handle cleanup</param>
        /// <param name="count">The number of instances to allocate space for</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>A pointer to the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 1</exception>
        /// <exception cref="ArgumentNullException">Thrown when deferScope is null</exception>
        public Pointer<T> AllocateDeferred<T>(DeferredCleanupScope deferScope, int count = 1, bool zeroed = false) where T : unmanaged
        {
            if (deferScope == null)
                throw new ArgumentNullException(nameof(deferScope));

            var pointer = Allocator.Allocate<T>(count, zeroed);
            var allocatorRef = Allocator;
            var pointerRaw = pointer.Raw;
            deferScope.DeferAction(() => allocatorRef.Free(pointerRaw));
            return pointer;
        }

        /// <summary>
        /// Allocates a slice that will be automatically freed when the defer scope is disposed.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="deferScope">The defer scope that will handle cleanup</param>
        /// <param name="count">The number of elements in the slice</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>A slice representing the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 0</exception>
        /// <exception cref="ArgumentNullException">Thrown when deferScope is null</exception>
        public Slice<T> AllocateSliceDeferred<T>(DeferredCleanupScope deferScope, int count, bool zeroed = false) where T : unmanaged
        {
            if (deferScope == null)
                throw new ArgumentNullException(nameof(deferScope));

            var slice = Allocator.AllocateSlice<T>(count, zeroed);
            var allocatorRef = Allocator;
            var pointerRaw = slice.Ptr.Raw;
            deferScope.DeferAction(() => allocatorRef.Free(pointerRaw));
            return slice;
        }

        /// <summary>
        /// Formats a string and allocates it as a UTF-8 byte slice with automatic cleanup.
        /// </summary>
        /// <param name="deferScope">The defer scope that will handle cleanup</param>
        /// <param name="format">The format string</param>
        /// <param name="args">The arguments to format</param>
        /// <returns>A slice containing the UTF-8 encoded formatted string with null terminator</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentNullException">Thrown when deferScope or format is null</exception>
        public Slice<byte> FormatToSliceDeferred(DeferredCleanupScope deferScope, string format, params object[] args)
        {
            if (deferScope == null)
                throw new ArgumentNullException(nameof(deferScope));
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            var formattedText = string.Format(format, args);
            int byteCount = Encoding.UTF8.GetByteCount(formattedText);
            var slice = Allocator.AllocateSlice<byte>(byteCount + 1); // +1 for null terminator
            
            Encoding.UTF8.GetBytes(formattedText, slice.AsSpan());
            slice[byteCount] = 0; // Null-terminated
            
            var allocatorRef = Allocator;
            var pointerRaw = slice.Ptr.Raw;
            deferScope.DeferAction(() => allocatorRef.Free(pointerRaw));
            
            return slice;
        }
    }
}