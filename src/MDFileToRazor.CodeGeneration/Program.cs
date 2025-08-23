using MDFIleTORazor.CodeGeneration;

if (args.Length != 2)
{
    Console.WriteLine("Usage: MDFileToRazor.CodeGeneration <sourceDirectory> <outputDirectory>");
    Console.WriteLine("  sourceDirectory: Directory containing markdown files to process");
    Console.WriteLine("  outputDirectory: Directory where generated Razor pages will be written");
    Environment.Exit(1);
}

string sourceDirectory = args[0];
string outputDirectory = args[1];

if (!Directory.Exists(sourceDirectory))
{
    Console.WriteLine($"Error: Source directory '{sourceDirectory}' does not exist.");
    Environment.Exit(1);
}

Console.WriteLine($"Processing markdown files from: {sourceDirectory}");
Console.WriteLine($"Generating Razor pages to: {outputDirectory}");

// Ensure output directory exists
Directory.CreateDirectory(outputDirectory);

var generator = new MarkdownToRazorGenerator();

try
{
    await generator.GenerateRazorPagesAsync(sourceDirectory, outputDirectory);
    Console.WriteLine("Processing completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during processing: {ex.Message}");
    Environment.Exit(1);
}
