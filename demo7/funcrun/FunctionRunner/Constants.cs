namespace FunctionRunner
{
    // color from https://github.com/aspnet/Logging/blob/release/2.2/src/Microsoft.Extensions.Logging.Console/Internal/AnsiLogConsole.cs
    static class ConsoleColor
    {
        public static string Black = "\x1B[30m";
        public static string Blue = "\x1B[1m\x1B[34m";
        public static string Cyan = "\x1B[1m\x1B[36m";
        public static string Green = "\x1B[1m\x1B[32m";
        public static string Red = "\x1B[31m";
        public static string White = "\x1B[1m\x1B[37m";
        public static string Yellow = "\x1B[1m\x1B[33m";

        public static string Default = "\x1B[39m\x1B[22m";
    }

    static class ConsoleBackgroundColor
    {
        public static string Red = "\x1B[41m";

        public static string Default = "\x1B[49m";
    }
}
