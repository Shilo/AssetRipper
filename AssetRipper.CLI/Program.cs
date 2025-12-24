using AssetRipper.GUI.Web;

// If command line arguments are not provided, show usage and exit
if (args.Length < 2)
{
	Console.WriteLine("Usage: AssetRipper.CLI <Game Path> <Export Path>");
	return;
}

string importGamePath = args[0];
string exportPath = args[1];

// If the game path does not exist, show error and exit
if (!System.IO.File.Exists(importGamePath) && !System.IO.Directory.Exists(importGamePath))
{
	Console.WriteLine($"Error: The specified game path '{importGamePath}' does not exist.");
	return;
}

// If export path is a file, show error and exit
if (System.IO.File.Exists(exportPath))
{
	Console.WriteLine($"Error: The specified export path '{exportPath}' is a file.");
	return;
}

// If export path is a non-empty directory, prompt user to continue
if (System.IO.Directory.Exists(exportPath))
{
	int fileCount = System.IO.Directory.GetFiles(exportPath).Length;
	int dirCount = System.IO.Directory.GetDirectories(exportPath).Length;
	if (fileCount > 0 || dirCount > 0)
	{
		Console.WriteLine($"The specified export path '{exportPath}' is a non-empty directory ({fileCount} files, {dirCount} folders).\nDo you want to continue? (y/n)");
		string input = Console.ReadLine() ?? "";
		if (!input.Trim().StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
		{
			return;
		}
	}
}

try
{
	Console.WriteLine($"Loading game path: {importGamePath}");
	GameFileLoader.LoadAndProcess([importGamePath]);
	_ = GameFileLoader.ExportUnityProject(exportPath);
	Console.WriteLine($"Export completed successfully to: {exportPath}");
}
catch (Exception ex)
{
	Console.WriteLine($"Error: {ex.Message}");
	Console.WriteLine(ex.StackTrace);
}
