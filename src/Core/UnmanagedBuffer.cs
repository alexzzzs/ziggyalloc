using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZiggyAlloc
{
    /// <summary>
    /// Represents a buffer of unmanaged memory with type-safe access and automatic cleanup options.
    /// </summary>
    /// <typeparam name="T">The unmanaged type stored in the buffer</typeparam>
    public unsafe class UnmanagedBuffer<T> : IDisposable where T : unmanaged
    {
        private T* _pointer;
        private int _length;
        private IUnmanagedMemoryAllocator? _allocator;
        private object? _pool; // Reference to the pool that owns this buffer
        private object? _managedArrayInfo; // Reference to managed array info for hybrid allocator
        private bool _ownsMemory;
        private bool _disposed = false;

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
        public IntPtr RawPointer => (IntPtr)_pointer;

        /// <summary>
        /// Gets the size of the buffer in bytes.
        /// </summary>
        public int SizeInBytes => _length * sizeof(T);

        /// <summary>
        /// Initializes a new buffer that owns its memory and will free it when disposed.
        /// </summary>
        internal UnmanagedBuffer(T* pointer, int length, IUnmanagedMemoryAllocator allocator)
        {
            _pointer = pointer;
            _length = length;
            _allocator = allocator;
            _pool = null;
            _managedArrayInfo = null;
            _ownsMemory = true;
        }

        /// <summary>
        /// Initializes a new buffer that wraps existing memory without owning it.
        /// </summary>
        public UnmanagedBuffer(T* pointer, int length)
        {
            _pointer = pointer;
            _length = length;
            _allocator = null;
            _pool = null;
            _managedArrayInfo = null;
            _ownsMemory = false;
        }

        /// <summary>
        /// Internal constructor for pool-based buffers.
        /// </summary>
        internal UnmanagedBuffer(T* pointer, int length, object pool)
        {
            _pointer = pointer;
            _length = length;
            _allocator = null;
            _pool = pool;
            _managedArrayInfo = null;
            _ownsMemory = true;
        }

        /// <summary>
        /// Internal constructor for managed array buffers from HybridAllocator.
        /// </summary>
        internal UnmanagedBuffer(T* pointer, int length, HybridAllocator.ManagedArrayInfo managedArrayInfo)
        {
            _pointer = pointer;
            _length = length;
            _allocator = null;
            _pool = null;
            _managedArrayInfo = managedArrayInfo;
            _ownsMemory = true;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfDisposed();
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
        public ref T First
        {
            get
            {
                ThrowIfDisposed();
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
        public ref T Last
        {
            get
            {
                ThrowIfDisposed();
                if (_pointer == null)
                    throw new InvalidOperationException("Buffer is not valid (null pointer)");
                
                if (_length == 0)
                    throw new InvalidOperationException("Buffer is empty");
                
                return ref _pointer[_length - 1];
            }
        }

        /// <summary>
        /// Converts the buffer to a Span<T> for high-performance operations.
        /// </summary>
        public Span<T> AsSpan()
        {
            ThrowIfDisposed();
            if (_pointer == null)
                throw new InvalidOperationException("Buffer is not valid (null pointer)");
            
            return new Span<T>(_pointer, _length);
        }

        /// <summary>
        /// Converts a portion of the buffer to a Span<T>.
        /// </summary>
        public Span<T> AsSpan(int start, int length)
        {
            ThrowIfDisposed();
            if (_pointer == null)
                throw new InvalidOperationException("Buffer is not valid (null pointer)");
            
            if (start < 0 || start > _length)
                throw new ArgumentOutOfRangeException(nameof(start));
            
            if (length < 0 || start + length > _length)
                throw new ArgumentOutOfRangeException(nameof(length));
            
            return new Span<T>(_pointer + start, length);
        }

        /// <summary>
        /// Converts the buffer to a ReadOnlySpan<T>.
        /// </summary>
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            ThrowIfDisposed();
            if (_pointer == null)
                throw new InvalidOperationException("Buffer is not valid (null pointer)");
            
            return new ReadOnlySpan<T>(_pointer, _length);
        }

        /// <summary>
        /// Fills the entire buffer with the specified value.
        /// </summary>
        public void Fill(T value)
        {
            ThrowIfDisposed();
            AsSpan().Fill(value);
        }

        /// <summary>
        /// Clears the buffer by setting all bytes to zero.
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();
            if (_pointer == null)
                throw new InvalidOperationException("Buffer is not valid (null pointer)");
            
            var byteSpan = new Span<byte>(_pointer, SizeInBytes);
            byteSpan.Clear();
        }

        /// <summary>
        /// Copies data from another buffer to this buffer.
        /// </summary>
        public void CopyFrom(UnmanagedBuffer<T> source)
        {
            ThrowIfDisposed();
            source.AsReadOnlySpan().CopyTo(AsSpan());
        }

        /// <summary>
        /// Copies data from a span to this buffer.
        /// </summary>
        public void CopyFrom(ReadOnlySpan<T> source)
        {
            ThrowIfDisposed();
            source.CopyTo(AsSpan());
        }

        /// <summary>
        /// Frees the buffer memory if it's owned by this instance.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_ownsMemory && _pointer != null)
            {
                if (_allocator != null)
                {
                    // Use regular allocator free
                    _allocator.Free((IntPtr)_pointer);
                }
                else if (_pool != null)
                {
                    // For pool-based buffers, we don't actually free the memory,
                    // but return it to the pool instead
                    // The pool will handle the actual freeing when it's disposed
                    if (_pool is IUnmanagedMemoryAllocator poolAllocator)
                    {
                        poolAllocator.Free((IntPtr)_pointer);
                    }
                }
                else if (_managedArrayInfo != null)
                {
                    // Free managed array by unpinning it
                    var info = (HybridAllocator.ManagedArrayInfo)_managedArrayInfo;
                    if (info.Handle.IsAllocated)
                    {
                        info.Handle.Free();
                    }
                }
            }

            _disposed = true;
            _pointer = null;
            _ownsMemory = false;
        }

        /// <summary>
        /// Implicitly converts the buffer to a Span&lt;T&gt;.
        /// </summary>
        public static implicit operator Span<T>(UnmanagedBuffer<T> buffer) => buffer.AsSpan();

        /// <summary>
        /// Implicitly converts the buffer to a ReadOnlySpan&lt;T&gt;.
        /// </summary>
        public static implicit operator ReadOnlySpan<T>(UnmanagedBuffer<T> buffer) => buffer.AsReadOnlySpan();

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UnmanagedBuffer<T>));
            }
        }
    }
}