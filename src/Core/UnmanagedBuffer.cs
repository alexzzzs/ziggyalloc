using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZiggyAlloc
{
    /// <summary>
    /// Represents a buffer of unmanaged memory with type-safe access and automatic cleanup options.
    /// </summary>
    /// <typeparam name="T">The unmanaged type stored in the buffer</typeparam>
    /// <remarks>
    /// This type provides a safe wrapper around unmanaged memory allocations, offering:
    /// - Type-safe access to unmanaged memory
    /// - Bounds checking for array access
    /// - Conversion to Span&lt;T&gt; for high-performance operations
    /// - Integration with native APIs through raw pointer access
    /// - Optional automatic cleanup through IDisposable
    /// 
    /// Primary use cases:
    /// - Interop with native libraries requiring contiguous memory
    /// - Large buffer allocations to avoid GC pressure
    /// - Performance-critical scenarios requiring direct memory control
    /// - Custom memory layout patterns (struct-of-arrays)
    /// </remarks>
    public unsafe struct UnmanagedBuffer<T> : IDisposable where T : unmanaged
    {
        private readonly T* _pointer;
        private readonly int _length;
        private readonly IUnmanagedMemoryAllocator? _allocator;
        private readonly bool _ownsMemory;

        /// <summary>
        /// Gets the number of elements in the buffer.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Gets a value indicating whether the buffer is empty (length is 0).
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        /// Gets a value indicating whether the buffer points to valid memory.
        /// </summary>
        public bool IsValid => _pointer != null;

        /// <summary>
        /// Gets the raw pointer to the buffer memory.
        /// </summary>
        /// <remarks>
        /// Use this for interop with native APIs. The pointer remains valid
        /// until the buffer is disposed (if it owns the memory).
        /// </remarks>
        public IntPtr RawPointer => (IntPtr)_pointer;

        /// <summary>
        /// Gets the size of the buffer in bytes.
        /// </summary>
        public int SizeInBytes => _length * sizeof(T);

        /// <summary>
        /// Initializes a new buffer that owns its memory and will free it when disposed.
        /// </summary>
        /// <param name="pointer">Pointer to the allocated memory</param>
        /// <param name="length">Number of elements in the buffer</param>
        /// <param name="allocator">The allocator used to allocate the memory</param>
        internal UnmanagedBuffer(T* pointer, int length, IUnmanagedMemoryAllocator allocator)
        {
            _pointer = pointer;
            _length = length;
            _allocator = allocator;
            _ownsMemory = true;
        }

        /// <summary>
        /// Initializes a new buffer that wraps existing memory without owning it.
        /// </summary>
        /// <param name="pointer">Pointer to the existing memory</param>
        /// <param name="length">Number of elements in the buffer</param>
        /// <remarks>
        /// The buffer will not free the memory when disposed. Use this for wrapping
        /// memory allocated elsewhere or for stack-allocated memory.
        /// </remarks>
        public UnmanagedBuffer(T* pointer, int length)
        {
            _pointer = pointer;
            _length = length;
            _allocator = null;
            _ownsMemory = false;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element</param>
        /// <returns>A reference to the element at the specified index</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when index is less than 0 or greater than or equal to Length
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is not valid (null pointer)
        /// </exception>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_pointer == null)
                    throw new InvalidOperationException("Buffer is not valid (null pointer)");
                
                if ((uint)index >= (uint)_length)
                    throw new IndexOutOfRangeException($"Index {index} is out of range [0, {_length})");
                
                return ref _pointer[index];
            }
        }

        /// <summary>
        /// Gets a reference to the first element in the buffer.
        /// </summary>
        /// <returns>A reference to the first element</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is empty or not valid
        /// </exception>
        public ref T First
        {
            get
            {
                if (_pointer == null)
                    throw new InvalidOperationException("Buffer is not valid (null pointer)");
                
                if (_length == 0)
                    throw new InvalidOperationException("Buffer is empty");
                
                return ref _pointer[0];
            }
        }

        /// <summary>
        /// Gets a reference to the last element in the buffer.
        /// </summary>
        /// <returns>A reference to the last element</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is empty or not valid
        /// </exception>
        public ref T Last
        {
            get
            {
                if (_pointer == null)
                    throw new InvalidOperationException("Buffer is not valid (null pointer)");
                
                if (_length == 0)
                    throw new InvalidOperationException("Buffer is empty");
                
                return ref _pointer[_length - 1];
            }
        }

        /// <summary>
        /// Converts the buffer to a Span&lt;T&gt; for high-performance operations.
        /// </summary>
        /// <returns>A span representing the buffer contents</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is not valid (null pointer)
        /// </exception>
        public Span<T> AsSpan()
        {
            if (_pointer == null)
                throw new InvalidOperationException("Buffer is not valid (null pointer)");
            
            return new Span<T>(_pointer, _length);
        }

        /// <summary>
        /// Converts a portion of the buffer to a Span&lt;T&gt;.
        /// </summary>
        /// <param name="start">The starting index</param>
        /// <param name="length">The number of elements to include</param>
        /// <returns>A span representing the specified portion of the buffer</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when start or length is invalid
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is not valid (null pointer)
        /// </exception>
        public Span<T> AsSpan(int start, int length)
        {
            if (_pointer == null)
                throw new InvalidOperationException("Buffer is not valid (null pointer)");
            
            if (start < 0 || start > _length)
                throw new ArgumentOutOfRangeException(nameof(start));
            
            if (length < 0 || start + length > _length)
                throw new ArgumentOutOfRangeException(nameof(length));
            
            return new Span<T>(_pointer + start, length);
        }

        /// <summary>
        /// Converts the buffer to a ReadOnlySpan&lt;T&gt;.
        /// </summary>
        /// <returns>A read-only span representing the buffer contents</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is not valid (null pointer)
        /// </exception>
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            if (_pointer == null)
                throw new InvalidOperationException("Buffer is not valid (null pointer)");
            
            return new ReadOnlySpan<T>(_pointer, _length);
        }

        /// <summary>
        /// Fills the entire buffer with the specified value.
        /// </summary>
        /// <param name="value">The value to fill the buffer with</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is not valid (null pointer)
        /// </exception>
        public void Fill(T value)
        {
            AsSpan().Fill(value);
        }

        /// <summary>
        /// Clears the buffer by setting all bytes to zero.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is not valid (null pointer)
        /// </exception>
        public void Clear()
        {
            if (_pointer == null)
                throw new InvalidOperationException("Buffer is not valid (null pointer)");
            
            var byteSpan = new Span<byte>(_pointer, SizeInBytes);
            byteSpan.Clear();
        }

        /// <summary>
        /// Copies data from another buffer to this buffer.
        /// </summary>
        /// <param name="source">The source buffer to copy from</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the source buffer is larger than this buffer
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when either buffer is not valid
        /// </exception>
        public void CopyFrom(UnmanagedBuffer<T> source)
        {
            source.AsReadOnlySpan().CopyTo(AsSpan());
        }

        /// <summary>
        /// Copies data from a span to this buffer.
        /// </summary>
        /// <param name="source">The source span to copy from</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the source span is larger than this buffer
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is not valid
        /// </exception>
        public void CopyFrom(ReadOnlySpan<T> source)
        {
            source.CopyTo(AsSpan());
        }

        /// <summary>
        /// Frees the buffer memory if it's owned by this instance.
        /// </summary>
        /// <remarks>
        /// After disposal, the buffer should not be used. If the buffer doesn't own
        /// its memory (created with the non-owning constructor), this method does nothing.
        /// </remarks>
        public void Dispose()
        {
            if (_ownsMemory && _allocator != null && _pointer != null)
            {
                _allocator.Free((IntPtr)_pointer);
            }
        }

        /// <summary>
        /// Implicitly converts the buffer to a Span&lt;T&gt;.
        /// </summary>
        /// <param name="buffer">The buffer to convert</param>
        /// <returns>A span representing the buffer contents</returns>
        public static implicit operator Span<T>(UnmanagedBuffer<T> buffer) => buffer.AsSpan();

        /// <summary>
        /// Implicitly converts the buffer to a ReadOnlySpan&lt;T&gt;.
        /// </summary>
        /// <param name="buffer">The buffer to convert</param>
        /// <returns>A read-only span representing the buffer contents</returns>
        public static implicit operator ReadOnlySpan<T>(UnmanagedBuffer<T> buffer) => buffer.AsReadOnlySpan();
    }
}