using System;
using System.Runtime.CompilerServices;

namespace ZiggyAlloc
{
    /// <summary>
    /// A buffer wrapper that provides proper alignment for hardware acceleration.
    /// </summary>
    /// <typeparam name="T">The unmanaged type stored in the buffer</typeparam>
    /// <remarks>
    /// AlignedBuffer provides a view into a base buffer with proper alignment padding.
    /// It tracks the original allocation for proper cleanup while presenting an aligned interface.
    /// </remarks>
    public unsafe readonly struct AlignedBuffer<T> : IDisposable where T : unmanaged
    {
        private readonly T* _alignedPointer;
        private readonly int _length;
        private readonly IUnmanagedMemoryAllocator _allocator;
        private readonly IntPtr _basePointer;
        private readonly int _paddingBytes;

        /// <summary>
        /// Gets the aligned pointer to the buffer data.
        /// </summary>
        public T* AlignedPointer => _alignedPointer;

        /// <summary>
        /// Gets the number of elements in the buffer.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Gets the size of the buffer in bytes.
        /// </summary>
        public int SizeInBytes => Length * sizeof(T);

        /// <summary>
        /// Gets the number of padding bytes used for alignment.
        /// </summary>
        public int PaddingBytes => _paddingBytes;

        /// <summary>
        /// Gets a value indicating whether the buffer is empty.
        /// </summary>
        public bool IsEmpty => Length == 0;

        /// <summary>
        /// Gets a value indicating whether the buffer is valid (has a non-null pointer).
        /// </summary>
        public bool IsValid => _alignedPointer != null;

        /// <summary>
        /// Gets a reference to the first element.
        /// </summary>
        public ref T First
        {
            get
            {
                if (!IsValid)
                    throw new InvalidOperationException("Buffer is not valid");
                return ref *_alignedPointer;
            }
        }

        /// <summary>
        /// Gets a reference to the last element.
        /// </summary>
        public ref T Last
        {
            get
            {
                if (!IsValid)
                    throw new InvalidOperationException("Buffer is not valid");
                return ref *(_alignedPointer + (Length - 1));
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index with bounds checking.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set</param>
        /// <returns>A reference to the element at the specified index</returns>
        public ref T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Length)
                    throw new IndexOutOfRangeException($"Index {index} is out of range [0, {Length})");

                return ref *(_alignedPointer + index);
            }
        }

        /// <summary>
        /// Initializes a new instance of the AlignedBuffer struct.
        /// </summary>
        /// <param name="alignedPointer">The aligned pointer to the buffer data</param>
        /// <param name="length">The number of elements in the buffer</param>
        /// <param name="allocator">The allocator that owns this buffer</param>
        /// <param name="basePointer">The original unaligned pointer for cleanup</param>
        /// <param name="paddingBytes">The number of padding bytes used for alignment</param>
        internal AlignedBuffer(T* alignedPointer, int length, IUnmanagedMemoryAllocator allocator,
                              IntPtr basePointer, int paddingBytes)
        {
            _alignedPointer = alignedPointer;
            _length = length;
            _allocator = allocator;
            _basePointer = basePointer;
            _paddingBytes = paddingBytes;

            // Track for proper cleanup
            if (basePointer != IntPtr.Zero && alignedPointer != null)
            {
                AlignedBufferTracker.Track((IntPtr)alignedPointer, basePointer);
            }
        }

        /// <summary>
        /// Creates an aligned buffer from a base buffer.
        /// </summary>
        /// <param name="baseBuffer">The base buffer to align</param>
        /// <param name="alignment">The required alignment</param>
        /// <returns>An aligned buffer wrapper</returns>
        public static AlignedBuffer<T> Create(UnmanagedBuffer<T> baseBuffer, int alignment)
        {
            if (!baseBuffer.IsValid)
                return new AlignedBuffer<T>(null, 0, null, IntPtr.Zero, 0);

            var baseAddress = (byte*)baseBuffer.RawPointer.ToPointer();
            var alignedAddress = (byte*)AlignPointer(baseAddress, alignment);

            int paddingBytes = (int)(alignedAddress - baseAddress);

            return new AlignedBuffer<T>(
                (T*)alignedAddress,
                baseBuffer.Length,
                null, // We'll handle cleanup differently
                baseBuffer.RawPointer,
                paddingBytes
            );
        }

        /// <summary>
        /// Aligns a pointer to the specified alignment boundary.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void* AlignPointer(void* pointer, int alignment)
        {
            var address = (nuint)pointer;
            var aligned = (address + (uint)(alignment - 1)) & ~(nuint)(alignment - 1);
            return (void*)aligned;
        }

        /// <summary>
        /// Converts the aligned buffer to a Span<T>.
        /// </summary>
        /// <returns>A span representing the aligned buffer data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return IsValid ? new Span<T>(_alignedPointer, Length) : Span<T>.Empty;
        }

        /// <summary>
        /// Converts the aligned buffer to a ReadOnlySpan<T>.
        /// </summary>
        /// <returns>A readonly span representing the aligned buffer data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return IsValid ? new ReadOnlySpan<T>(_alignedPointer, Length) : ReadOnlySpan<T>.Empty;
        }

        /// <summary>
        /// Fills the buffer with the specified value.
        /// </summary>
        /// <param name="value">The value to fill the buffer with</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(T value)
        {
            if (IsValid)
            {
                AsSpan().Fill(value);
            }
        }

        /// <summary>
        /// Clears the buffer by setting all bytes to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (IsValid)
            {
                AsSpan().Clear();
            }
        }

        /// <summary>
        /// Copies data from the specified span into this buffer.
        /// </summary>
        /// <param name="source">The source span to copy from</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ReadOnlySpan<T> source)
        {
            if (IsValid)
            {
                source.CopyTo(AsSpan());
            }
        }

        /// <summary>
        /// Copies data from this buffer to the specified span.
        /// </summary>
        /// <param name="destination">The destination span to copy to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> destination)
        {
            if (IsValid)
            {
                AsSpan().CopyTo(destination);
            }
        }

        /// <summary>
        /// Implicit conversion to Span<T> for zero-cost interoperability.
        /// </summary>
        /// <param name="buffer">The aligned buffer to convert</param>
        /// <returns>A span representing the aligned buffer data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(AlignedBuffer<T> buffer)
        {
            return buffer.AsSpan();
        }

        /// <summary>
        /// Implicit conversion to ReadOnlySpan<T> for zero-cost interoperability.
        /// </summary>
        /// <param name="buffer">The aligned buffer to convert</param>
        /// <returns>A readonly span representing the aligned buffer data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(AlignedBuffer<T> buffer)
        {
            return buffer.AsReadOnlySpan();
        }

        /// <summary>
        /// Disposes the buffer by freeing the underlying allocation.
        /// </summary>
        public void Dispose()
        {
            if (_basePointer != IntPtr.Zero && _allocator != null)
            {
                _allocator.Free(_basePointer);
            }
        }

        /// <summary>
        /// Gets a string representation of the aligned buffer.
        /// </summary>
        /// <returns>A string describing the buffer's properties</returns>
        public override string ToString()
        {
            return $"AlignedBuffer<{typeof(T).Name}>(Length={Length}, AlignedPointer=0x{(ulong)_alignedPointer:X}, Padding={PaddingBytes} bytes)";
        }
    }
}