using AssetRipper.GUI.Web;
using AssetRipper.Import.Logging;
using System.Diagnostics;

// Check for --open-export flag
bool openExport = args.Contains("--open-export", StringComparer.OrdinalIgnoreCase);

// Filter out the flag to get the actual arguments
string[] pathArgs = args.Where(arg => !arg.Equals("--open-export", StringComparison.OrdinalIgnoreCase)).ToArray();

// If command line arguments are not provided, show usage and exit
if (pathArgs.Length < 2)
{
	CLILogger.Error("Usage: AssetRipper.CLI <Game Path> <Export Path> [--open-export]");
	CLILogger.Error("       --open-export can be placed anywhere in the command line");
	return;
}

string importGamePath = pathArgs[0];
string exportPath = pathArgs[1];

// If the game path does not exist, show error and exit
if (!System.IO.File.Exists(importGamePath) && !System.IO.Directory.Exists(importGamePath))
{
	CLILogger.Error($"The specified game path '{importGamePath}' does not exist.");
	return;
}

// If export path is a file, show error and exit
if (System.IO.File.Exists(exportPath))
{
	CLILogger.Error($"The specified export path '{exportPath}' is a file.");
	return;
}

// If export path is a non-empty directory, prompt user to continue
if (System.IO.Directory.Exists(exportPath))
{
	int fileCount = System.IO.Directory.GetFiles(exportPath, "*", System.IO.SearchOption.AllDirectories).Length;
	int dirCount = System.IO.Directory.GetDirectories(exportPath, "*", System.IO.SearchOption.AllDirectories).Length;
	if (fileCount > 0 || dirCount > 0)
	{
		CLILogger.Warning($"The selected export directory already exists and everything it contains will be deleted.");
		CLILogger.Warning($"Directory: {exportPath}");
		CLILogger.Warning($"Contents: {fileCount} files, {dirCount} folders");
		string input = CLILogger.Prompt("Are you sure you want to continue? (y/n)");
		if (!input.Trim().StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
		{
			CLILogger.Info("Operation cancelled by user.");
			return;
		}

		// Delete the existing directory
		try
		{
			System.IO.Directory.Delete(exportPath, recursive: true);
			CLILogger.Info("Export directory deleted successfully.");
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
	CLILogger.Info($"Loading game path \"{importGamePath}\"...");
	GameFileLoader.LoadAndProcess([importGamePath]);
	if (!GameFileLoader.IsLoaded)
	{
		CLILogger.Error("Failed to load game path.");
		Environment.ExitCode = 1;
		return;
	}

	CLILogger.Info($"Exporting game to \"{exportPath}\"...");

	bool exportSuccess = await GameFileLoader.ExportUnityProject(exportPath);
	if (exportSuccess)
	{
		CLILogger.Success($"Export completed successfully to: {exportPath}");

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
				CLILogger.Warning($"Failed to open export folder: {ex.Message}");
			}
		}
	}
	else
	{
		CLILogger.Error("Export was cancelled or failed.");
		Environment.ExitCode = 1;
	}
}
catch (Exception ex)
{
	CLILogger.Error($"An error occurred: {ex.Message}");
	var originalColor = Console.ForegroundColor;
	Console.ForegroundColor = ConsoleColor.DarkRed;
	Console.WriteLine(ex.StackTrace);
	Console.ForegroundColor = originalColor;
	Environment.ExitCode = 1;
}
