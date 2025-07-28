using System;

namespace ZiggyAlloc
{
    public interface IWriter
    {
        void Write(string s);
        void Write(char c);
        void WriteLine();
        void WriteLine<T>(T value);
    }

    public interface IReader
    {
        string? ReadLine();
        int Read();
    }

    public class ConsoleWriter : IWriter
    {
        public void Write(string s) => Console.Write(s);
        public void Write(char c) => Console.Write(c);
        public void WriteLine() => Console.WriteLine();
        public void WriteLine<T>(T v) => Console.WriteLine(v);
    }

    public class ConsoleReader : IReader
    {
        public string? ReadLine() => Console.ReadLine();
        public int Read() => Console.Read();
    }
}