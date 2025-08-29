using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class DeferScopeAdditionalTests
    {
        [Fact]
        public void DeferScope_DisposeMultipleTimes_HandledGracefully()
        {
            // Arrange
            var defer = DeferScope.Start();
            defer.Defer(() => { });
            
            // Act & Assert
            defer.Dispose(); // First disposal
            defer.Dispose(); // Second disposal should not throw
            
            // Test passes if no exception thrown
        }

        [Fact]
        public void DeferScope_DeferAfterDispose_ThrowsInvalidOperationException()
        {
            // Arrange
            var defer = DeferScope.Start();
            defer.Dispose(); // Dispose first
            
            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => defer.Defer(() => { }));
        }

        [Fact]
        public void DeferScope_LargeNumberOfDeferActions_HandledCorrectly()
        {
            // Arrange
            using var defer = DeferScope.Start();
            const int actionCount = 1000;
            var executionOrder = new List<int>();
            
            // Act - Add many defer actions
            for (int i = 0; i < actionCount; i++)
            {
                int capturedValue = i;
                defer.Defer(() => executionOrder.Add(capturedValue));
            }
            
            // Assert - All actions should be added
            Assert.Equal(actionCount, defer.Count);
        }

        [Fact]
        public void DeferScope_DeferredAllocation_WithDifferentTypes()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            
            // Act & Assert
            using (var defer = DeferScope.Start())
            {
                var intBuffer = allocator.AllocateDeferred<int>(defer, 100);
                var doubleBuffer = allocator.AllocateDeferred<double>(defer, 50);
                var byteBuffer = allocator.AllocateDeferred<byte>(defer, 200);
                
                // Use buffers
                intBuffer[0] = 42;
                doubleBuffer[0] = 3.14;
                byteBuffer[0] = 255;
                
                // Assert buffers are valid and accessible
                Assert.Equal(42, intBuffer[0]);
                Assert.Equal(3.14, doubleBuffer[0]);
                Assert.Equal((byte)255, byteBuffer[0]);
            } // All buffers should be automatically disposed here
            
            // After the scope ends, the buffers have been disposed
            // We can't safely access them, but we can verify the defer scope worked correctly
        }

        [Fact]
        public void DeferScope_NestedScopes_WithManyLevels()
        {
            // Arrange & Act
            var executionOrder = new List<string>();
            
            using (var level1 = DeferScope.Start())
            {
                level1.Defer(() => executionOrder.Add("level1-end"));
                
                using (var level2 = DeferScope.Start())
                {
                    level2.Defer(() => executionOrder.Add("level2-end"));
                    
                    using (var level3 = DeferScope.Start())
                    {
                        level3.Defer(() => executionOrder.Add("level3-end"));
                        
                        using (var level4 = DeferScope.Start())
                        {
                            level4.Defer(() => executionOrder.Add("level4-end"));
                            
                            using (var level5 = DeferScope.Start())
                            {
                                level5.Defer(() => executionOrder.Add("level5-end"));
                            } // level5 dispose
                        } // level4 dispose
                    } // level3 dispose
                } // level2 dispose
            } // level1 dispose

            // Assert - Actions should execute in correct LIFO order
            Assert.Equal(5, executionOrder.Count);
            Assert.Equal("level5-end", executionOrder[0]);
            Assert.Equal("level4-end", executionOrder[1]);
            Assert.Equal("level3-end", executionOrder[2]);
            Assert.Equal("level2-end", executionOrder[3]);
            Assert.Equal("level1-end", executionOrder[4]);
        }

        [Fact]
        public void DeferScope_ConcurrentUsage_ThreadSafe()
        {
            // Arrange
            const int threadCount = 10;
            const int scopesPerThread = 100;
            var tasks = new Task[threadCount];
            
            // Act - Run defer scopes in parallel
            for (int t = 0; t < threadCount; t++)
            {
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < scopesPerThread; i++)
                    {
                        using var defer = DeferScope.Start();
                        var executionLog = new List<string>();
                        
                        // Add defer actions
                        defer.Defer(() => executionLog.Add($"action1-{t}-{i}"));
                        defer.Defer(() => executionLog.Add($"action2-{t}-{i}"));
                        
                        // Assert
                        Assert.Equal(2, defer.Count);
                    }
                });
            }
            
            // Wait for all tasks to complete
            Task.WaitAll(tasks);
            
            // Assert - No exceptions should have been thrown
            Assert.True(true);
        }

        [Fact]
        public void DeferScope_ExceptionInMultipleDeferActions_AllExecute()
        {
            // Arrange
            var executionOrder = new List<string>();
            
            // Act & Assert
            var exceptionCount = 0;
            try
            {
                using (var defer = DeferScope.Start())
                {
                    defer.Defer(() => executionOrder.Add("first"));
                    defer.Defer(() => { throw new InvalidOperationException("Test exception 1"); });
                    defer.Defer(() => executionOrder.Add("third"));
                    defer.Defer(() => { throw new InvalidOperationException("Test exception 2"); });
                    defer.Defer(() => executionOrder.Add("fifth"));
                }
            }
            catch (AggregateException ex)
            {
                exceptionCount = ex.InnerExceptions.Count;
            }

            // Assert
            Assert.Equal(2, exceptionCount);
            Assert.Equal(3, executionOrder.Count);
            Assert.Equal("fifth", executionOrder[0]);
            Assert.Equal("third", executionOrder[1]);
            Assert.Equal("first", executionOrder[2]);
        }

        [Fact]
        public void DeferScope_StructTypeDeferredAllocation_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            
            // Act & Assert
            using (var defer = DeferScope.Start())
            {
                var buffer = allocator.AllocateDeferred<Point3D>(defer, 50);
                
                // Fill with data
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = new Point3D { X = i, Y = i * 2, Z = i * 3 };
                }
                
                // Assert buffer is valid and accessible
                Assert.True(buffer.IsValid);
                Assert.Equal(50, buffer.Length);
                Assert.Equal(0, buffer[0].X);
                Assert.Equal(4, buffer[2].Y);
                Assert.Equal(15, buffer[5].Z);
            } // Buffer should be automatically disposed here
        }

        public struct Point3D
        {
            public float X;
            public float Y;
            public float Z;
        }
    }
}