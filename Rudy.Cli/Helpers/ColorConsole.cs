namespace Rudy.Cli.Helpers;

public static class ColorConsole
{
    public static void Write(string msg, ConsoleColor color)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(msg);
        Console.ForegroundColor = old;
    }

    public static void WriteInfo(string msg) => Write(msg, ConsoleColor.Yellow);
    public static void WriteSuccess(string msg) => Write(msg, ConsoleColor.Green);
    public static void WriteError(string msg) => Write(msg, ConsoleColor.Red);
}