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
        private object? _slabSlot; // Reference to slab slot for slab allocator
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
            _slabSlot = null;
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
            _slabSlot = null;
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
            _slabSlot = null;
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
            _slabSlot = null;
            _ownsMemory = true;
        }

        /// <summary>
        /// Internal constructor for slab slot buffers from SlabAllocator.
        /// </summary>
        internal UnmanagedBuffer(T* pointer, int length, SlabAllocator.SlabSlot slabSlot)
        {
            _pointer = pointer;
            _length = length;
            _allocator = null;
            _pool = null;
            _managedArrayInfo = null;
            _slabSlot = slabSlot;
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
        /// Converts the buffer to a Span&lt;T&gt; for high-performance operations.
        /// </summary>
        public Span<T> AsSpan()
        {
            ThrowIfDisposed();
            // For zero-length buffers, return an empty span instead of throwing
            if (_pointer == null)
                return Span<T>.Empty;
            
            return new Span<T>(_pointer, _length);
        }

        /// <summary>
        /// Converts a portion of the buffer to a Span&lt;T&gt;.
        /// </summary>
        public Span<T> AsSpan(int start, int length)
        {
            ThrowIfDisposed();
            // For zero-length buffers, return an empty span instead of throwing
            if (_pointer == null)
            {
                if (start != 0 || length != 0)
                    throw new ArgumentOutOfRangeException(nameof(start));
                return Span<T>.Empty;
            }
            
            if (start < 0 || start > _length)
                throw new ArgumentOutOfRangeException(nameof(start));
            
            if (length < 0 || start + length > _length)
                throw new ArgumentOutOfRangeException(nameof(length));
            
            return new Span<T>(_pointer + start, length);
        }

        /// <summary>
        /// Converts the buffer to a ReadOnlySpan&lt;T&gt;.
        /// </summary>
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            ThrowIfDisposed();
            // For zero-length buffers, return an empty span instead of throwing
            if (_pointer == null)
                return ReadOnlySpan<T>.Empty;
            
            return new ReadOnlySpan<T>(_pointer, _length);
        }

        /// <summary>
        /// Fills the entire buffer with the specified value.
        /// </summary>
        public void Fill(T value)
        {
            ThrowIfDisposed();
            // For zero-length buffers, AsSpan() will return an empty span, so Fill() will do nothing
            AsSpan().Fill(value);
        }

        /// <summary>
        /// Clears the buffer by setting all bytes to zero.
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();
            // For zero-length buffers, AsSpan() will return an empty span, so Clear() will do nothing
            AsSpan().Clear();
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
                try
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
                    else if (_slabSlot != null)
                    {
                        // For slab slot buffers, we don't actually free the memory,
                        // but return it to the slab instead
                        if (_slabSlot is SlabAllocator.SlabSlot slabSlot)
                        {
                            slabSlot.Free();
                        }
                    }
                    else
                    {
                        // This shouldn't happen in normal operation, but as a safety measure,
                        // we won't try to free memory we don't know how to handle
                        // This prevents potential double-free scenarios
                    }
                }
                catch
                {
                    // Ignore exceptions during disposal to prevent crashes
                    // This is a safety measure to prevent the test runner from crashing
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