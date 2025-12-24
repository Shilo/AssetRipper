namespace AssetRipper.CLI;

public static class Logger
{
    private const string SuccessIcon = "[+]";
    private const string ErrorIcon = "[X]";
    private const string InfoIcon = "[>]";
    private const string WarningIcon = "[!]";
    private const string PromptIcon = "[?]";

    public static void Info(string message)
    {
        Log(message, InfoIcon, ConsoleColor.Cyan);
    }

    public static void Success(string message)
    {
        Log(message, SuccessIcon, ConsoleColor.Green);
    }

    public static void Error(string message)
    {
        Log(message, ErrorIcon, ConsoleColor.Red);
    }

    public static void Warning(string message)
    {
        Log(message, WarningIcon, ConsoleColor.Yellow);
    }

    public static string Prompt(string message)
    {
        Log(message, PromptIcon, ConsoleColor.Magenta);
        return Console.ReadLine() ?? "";
    }

    private static void Log(string message, string icon, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"{icon} {message}");
        Console.ForegroundColor = originalColor;
    }
}
