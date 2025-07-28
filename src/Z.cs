namespace ZiggyAlloc
{
    public static class Z
    {
        public static readonly ManualAllocator DefaultAllocator = new();
        public static readonly Ctx ctx = new(DefaultAllocator, new ConsoleWriter(), new ConsoleReader());
    }
}