using AssetRipper.CLI;
using AssetRipper.GUI.Web;
using System.Diagnostics;

// Check for --open-export flag
bool openExport = args.Contains("--open-export", StringComparer.OrdinalIgnoreCase);

// Filter out the flag to get the actual arguments
string[] pathArgs = args.Where(arg => !arg.Equals("--open-export", StringComparison.OrdinalIgnoreCase)).ToArray();

// If command line arguments are not provided, show usage and exit
if (pathArgs.Length < 2)
{
	Logger.Error("Usage: AssetRipper.CLI <Game Path> <Export Path> [--open-export]");
	Logger.Error("       --open-export can be placed anywhere in the command line");
	return;
}

string importGamePath = pathArgs[0];
string exportPath = pathArgs[1];

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

		if (openExport)
		{
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = "explorer.exe",
					Arguments = exportPath,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				Logger.Warning($"Failed to open export folder: {ex.Message}");
			}
		}
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
