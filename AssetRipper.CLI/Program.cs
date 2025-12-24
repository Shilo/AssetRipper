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
	int fileCount = System.IO.Directory.GetFiles(exportPath, "*", System.IO.SearchOption.AllDirectories).Length;
	int dirCount = System.IO.Directory.GetDirectories(exportPath, "*", System.IO.SearchOption.AllDirectories).Length;
	if (fileCount > 0 || dirCount > 0)
	{
		Logger.Warning($"The selected export directory already exists and everything it contains will be deleted.");
		Logger.Warning($"Directory: {exportPath}");
		Logger.Warning($"Contents: {fileCount} files, {dirCount} folders");
		string input = Logger.Prompt("Are you sure you want to continue? (y/n)");
		if (!input.Trim().StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
		{
			Logger.Info("Operation cancelled by user.");
			return;
		}

		// Delete the existing directory
		try
		{
			System.IO.Directory.Delete(exportPath, recursive: true);
			Logger.Info("Export directory deleted successfully.");
		}
		catch
		{
			// Silent fail because AssetRipper will prompt
			// user to delete the directory as fallback.
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
