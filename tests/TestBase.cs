using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    /// <summary>
    /// Base class for tests that provides defensive cleanup functionality.
    /// </summary>
    public class TestBase : IDisposable
    {
        private readonly SystemMemoryAllocator _allocator;
        private bool _disposed = false;

        public TestBase()
        {
            _allocator = new SystemMemoryAllocator();
            TestCleanup.RegisterAllocator(_allocator);
        }

        /// <summary>
        /// Gets a system memory allocator for use in tests.
        /// </summary>
        protected SystemMemoryAllocator Allocator => _allocator;

        /// <summary>
        /// Creates a buffer and registers it for cleanup and leak detection.
        /// </summary>
        protected UnmanagedBuffer<T> CreateBuffer<T>(int elementCount, bool zeroMemory = false) where T : unmanaged
        {
            var buffer = _allocator.Allocate<T>(elementCount, zeroMemory);
            TestCleanup.RegisterBuffer(buffer);
            TestCleanup.RegisterAllocation(buffer.RawPointer);
            return buffer;
        }

        /// <summary>
        /// Creates a buffer using defer pattern and registers it for cleanup.
        /// </summary>
        protected UnmanagedBuffer<T> CreateDeferredBuffer<T>(DeferScope defer, int elementCount, bool zeroMemory = false) where T : unmanaged
        {
            var buffer = _allocator.AllocateDeferred<T>(defer, elementCount, zeroMemory);
            TestCleanup.RegisterBuffer(buffer);
            return buffer;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Perform comprehensive cleanup with leak detection
                TestCleanup.ComprehensiveCleanup();
            }

            _disposed = true;
        }

        ~TestBase()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Test collection that ensures cleanup between test classes.
    /// </summary>
    [CollectionDefinition("TestCleanup")]
    public class TestCleanupCollection : ICollectionFixture<TestCleanupFixture>
    {
    }

    /// <summary>
    /// Fixture that performs cleanup after all tests in a collection complete.
    /// </summary>
    public class TestCleanupFixture : IDisposable
    {
        public TestCleanupFixture()
        {
            // Register for process exit cleanup
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        public void Dispose()
        {
            // Perform comprehensive final cleanup
            TestCleanup.ComprehensiveCleanup();

            // Unregister event handler
            AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        }

        private static void OnProcessExit(object? sender, EventArgs e)
        {
            // Comprehensive final cleanup when process exits
            TestCleanup.ComprehensiveCleanup();
        }
    }
}