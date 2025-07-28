using System;
using System.IO;
using System.Text;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class ContextTests
    {
        [Fact]
        public void Ctx_BasicOperations_Work()
        {
            var allocator = new ManualAllocator();
            var writer = new StringWriter();
            var reader = new StringReader("test input\n");
            var ctx = new Ctx(allocator, new TestWriter(writer), new TestReader(reader));

            // Test allocation methods
            var ptr = ctx.Alloc<int>();
            ptr.Value = 42;
            Assert.Equal(42, ptr.Value);

            var slice = ctx.AllocSlice<int>(3, zeroed: true);
            Assert.Equal(3, slice.Length);
            Assert.Equal(0, slice[0]);

            // Test auto allocation
            using (var auto = ctx.Auto<double>())
            {
                auto.Value = 3.14;
                Assert.Equal(3.14, auto.Value);
            }

            // Test I/O
            ctx.Print("Hello");
            ctx.PrintLine(" World");
            Assert.Contains("Hello World", writer.ToString());

            // Clean up
            ctx.alloc.Free(ptr.Raw);
            ctx.alloc.Free(slice.Ptr.Raw);
        }

        [Fact]
        public void Ctx_DeferredAllocations_AreFreedAutomatically()
        {
            var allocator = new ManualAllocator();
            var writer = new StringWriter();
            var reader = new StringReader("");
            var ctx = new Ctx(allocator, new TestWriter(writer), new TestReader(reader));

            using (var defer = DeferScope.Start())
            {
                var ptr = ctx.Alloc<int>(defer);
                ptr.Value = 123;
                Assert.Equal(123, ptr.Value);

                var slice = ctx.AllocSlice<byte>(defer, 10, zeroed: true);
                slice[0] = 255;
                Assert.Equal(255, slice[0]);
            } // Memory should be freed here automatically
        }

        [Fact]
        public void Ctx_FormatToSlice_CreatesFormattedByteSlice()
        {
            var allocator = new ManualAllocator();
            var writer = new StringWriter();
            var reader = new StringReader("");
            var ctx = new Ctx(allocator, new TestWriter(writer), new TestReader(reader));

            using (var defer = DeferScope.Start())
            {
                var slice = ctx.FormatToSlice(defer, "Hello, {0}! Number: {1}", "World", 42);
                
                // Convert to string (excluding null terminator)
                var result = Encoding.UTF8.GetString(slice.AsSpan()[..^1]);
                Assert.Equal("Hello, World! Number: 42", result);
                
                // Check null terminator
                Assert.Equal(0, slice[slice.Length - 1]);
            }
        }

        private class TestWriter : IWriter
        {
            private readonly TextWriter _writer;

            public TestWriter(TextWriter writer) => _writer = writer;

            public void Write(string s) => _writer.Write(s);
            public void Write(char c) => _writer.Write(c);
            public void WriteLine() => _writer.WriteLine();
            public void WriteLine<T>(T value) => _writer.WriteLine(value);
        }

        private class TestReader : IReader
        {
            private readonly TextReader _reader;

            public TestReader(TextReader reader) => _reader = reader;

            public string? ReadLine() => _reader.ReadLine();
            public int Read() => _reader.Read();
        }
    }
}