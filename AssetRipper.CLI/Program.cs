using AssetRipper.CLI;
using AssetRipper.GUI.Web;

// If command line arguments are not provided, show usage and exit
if (args.Length < 2)
{
	Logger.Error("Usage: AssetRipper.CLI <Game Path> <Export Path>");
	return;
}

string importGamePath = args[0];
string exportPath = args[1];

// If the game path does not exist, show error and exit
if (!System.IO.File.Exists(importGamePath) && !System.IO.Directory.Exists(importGamePath))
{
	Logger.Error($"The specified game path '{importGamePath}' does not exist.");
	return;
}

// If export path is a file, show error and exit
if (System.IO.File.Exists(exportPath))
{
	Logger.Error($"The specified export path '{exportPath}' is a file.");
	return;
}

// If export path is a non-empty directory, prompt user to continue
if (System.IO.Directory.Exists(exportPath))
{
	int fileCount = System.IO.Directory.GetFiles(exportPath).Length;
	int dirCount = System.IO.Directory.GetDirectories(exportPath).Length;
	if (fileCount > 0 || dirCount > 0)
	{
		Logger.Warning($"The specified export path '{exportPath}' is a non-empty directory ({fileCount} files, {dirCount} folders).");
		string input = Logger.Prompt("Do you want to continue? (y/n)");
		if (!input.Trim().StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
		{
			Logger.Info("Operation cancelled by user.");
			return;
		}
	}
}

try
{
	Logger.Info($"Loading game path: {importGamePath}");
	GameFileLoader.LoadAndProcess([importGamePath]);
	if (!GameFileLoader.IsLoaded)
	{
		Logger.Error("Failed to load game path.");
		Environment.ExitCode = 1;
		return;
	}

	bool exportSuccess = await GameFileLoader.ExportUnityProject(exportPath);
	if (exportSuccess)
	{
		Logger.Success($"Export completed successfully to: {exportPath}");
	}
	else
	{
		Logger.Error("Export was cancelled or failed.");
		Environment.ExitCode = 1;
	}
}
catch (Exception ex)
{
	Logger.Error($"An error occurred: {ex.Message}");
	var originalColor = Console.ForegroundColor;
	Console.ForegroundColor = ConsoleColor.DarkRed;
	Console.WriteLine(ex.StackTrace);
	Console.ForegroundColor = originalColor;
	Environment.ExitCode = 1;
}
