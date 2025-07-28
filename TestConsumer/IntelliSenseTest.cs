using ZiggyAlloc;

// This file tests that all public APIs are accessible and documented
public class IntelliSenseTest
{
    public void TestApiAccess()
    {
        // Test that all main types are accessible
        var manual = new ManualAllocator();
        var scoped = new ScopedAllocator();
        var debug = new DebugAllocator("test", manual);
        
        // Test context system
        var ctx = new Ctx(manual, new ConsoleWriter(), new ConsoleReader());
        
        // Test defer scope
        var defer = DeferScope.Start();
        
        // Test core types
        var ptr = manual.Alloc<int>();
        var slice = manual.Slice<byte>(10);
        
        // Test static helpers
        var defaultCtx = Z.ctx;
        var defaultAlloc = Z.DefaultAllocator;
        
        // Test enums
        var mode = LeakReportingMode.Log;
        
        // Cleanup
        manual.Free(ptr.Raw);
        manual.Free(slice.Ptr.Raw);
        defer.Dispose();
        debug.Dispose();
        scoped.Dispose();
    }
}