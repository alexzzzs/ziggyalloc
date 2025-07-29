using System;

namespace ZiggyAlloc
{
    /// <summary>
    /// Defines the contract for output operations in ZiggyAlloc contexts.
    /// </summary>
    /// <remarks>
    /// This interface abstracts output operations, allowing contexts to work with
    /// different output targets (console, files, strings, etc.) without being
    /// tightly coupled to any specific implementation.
    /// </remarks>
    public interface IOutputWriter
    {
        /// <summary>
        /// Writes a string to the output without a trailing newline.
        /// </summary>
        /// <param name="text">The text to write</param>
        void Write(string text);

        /// <summary>
        /// Writes a single character to the output without a trailing newline.
        /// </summary>
        /// <param name="character">The character to write</param>
        void Write(char character);

        /// <summary>
        /// Writes a newline to the output.
        /// </summary>
        void WriteLine();

        /// <summary>
        /// Writes the string representation of a value followed by a newline.
        /// </summary>
        /// <typeparam name="T">The type of the value to write</typeparam>
        /// <param name="value">The value to write</param>
        void WriteLine<T>(T value);
    }

    /// <summary>
    /// Defines the contract for input operations in ZiggyAlloc contexts.
    /// </summary>
    /// <remarks>
    /// This interface abstracts input operations, allowing contexts to work with
    /// different input sources (console, files, strings, etc.) without being
    /// tightly coupled to any specific implementation.
    /// </remarks>
    public interface IInputReader
    {
        /// <summary>
        /// Reads a line of text from the input source.
        /// </summary>
        /// <returns>The line of text read, or null if end of input is reached</returns>
        string? ReadLine();

        /// <summary>
        /// Reads a single character from the input source.
        /// </summary>
        /// <returns>The character read, or -1 if end of input is reached</returns>
        int Read();
    }

    /// <summary>
    /// An output writer implementation that writes to the console.
    /// </summary>
    /// <remarks>
    /// This implementation delegates all operations to the standard Console class,
    /// making it suitable for command-line applications and interactive scenarios.
    /// </remarks>
    public class ConsoleOutputWriter : IOutputWriter
    {
        /// <summary>
        /// Writes a string to the console without a trailing newline.
        /// </summary>
        /// <param name="text">The text to write to the console</param>
        public void Write(string text) => Console.Write(text);

        /// <summary>
        /// Writes a single character to the console without a trailing newline.
        /// </summary>
        /// <param name="character">The character to write to the console</param>
        public void Write(char character) => Console.Write(character);

        /// <summary>
        /// Writes a newline to the console.
        /// </summary>
        public void WriteLine() => Console.WriteLine();

        /// <summary>
        /// Writes the string representation of a value to the console followed by a newline.
        /// </summary>
        /// <typeparam name="T">The type of the value to write</typeparam>
        /// <param name="value">The value to write to the console</param>
        public void WriteLine<T>(T value) => Console.WriteLine(value);
    }

    /// <summary>
    /// An input reader implementation that reads from the console.
    /// </summary>
    /// <remarks>
    /// This implementation delegates all operations to the standard Console class,
    /// making it suitable for command-line applications and interactive scenarios.
    /// </remarks>
    public class ConsoleInputReader : IInputReader
    {
        /// <summary>
        /// Reads a line of text from the console.
        /// </summary>
        /// <returns>The line of text read from the console, or null if end of input is reached</returns>
        public string? ReadLine() => Console.ReadLine();

        /// <summary>
        /// Reads a single character from the console.
        /// </summary>
        /// <returns>The character read from the console, or -1 if end of input is reached</returns>
        public int Read() => Console.Read();
    }
}