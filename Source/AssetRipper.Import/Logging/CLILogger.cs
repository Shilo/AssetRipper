namespace AssetRipper.Import.Logging;

public static class CLILogger
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

    public static void LogWithCategory(LogType type, LogCategory category, string message)
    {
        string typeString = type.ToString();
        string categoryString = category == LogCategory.None ? "None" : category.ToString();
        (string icon, ConsoleColor color) = GetIconAndColorForType(typeString);
        string categoryPrefix = string.IsNullOrEmpty(categoryString) || categoryString == "None" ? "" : $"[{categoryString}] ";
        WriteColored($"{icon} {categoryPrefix}{message}", color);
    }

    private static void Log(string message, string icon, ConsoleColor color)
    {
        WriteColored($"{icon} {message}", color);
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = originalColor;
    }

    private static (string icon, ConsoleColor color) GetIconAndColorForType(string type)
    {
        return type switch
        {
            "Error" => (ErrorIcon, ConsoleColor.Red),
            "Warning" => (WarningIcon, ConsoleColor.Yellow),
            "Debug" => (InfoIcon, ConsoleColor.Cyan),
            "Verbose" => (InfoIcon, ConsoleColor.Cyan),
            "Info" => (InfoIcon, ConsoleColor.Cyan),
            _ => (InfoIcon, ConsoleColor.Cyan),
        };
    }
}

