using MDFIleTORazor.CodeGeneration;

namespace MDFIleTORazor.CodeGeneration;

/// <summary>
/// Console application entry point for the code generator
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("MDFileToRazor Code Generator");
        Console.WriteLine("============================");

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: MDFIleTORazor.CodeGeneration <source-directory> <output-directory>");
            Console.WriteLine("Example: MDFIleTORazor.CodeGeneration ./mdFilesToConvert ./Pages/Generated");
            Environment.Exit(1);
        }

        var sourceDirectory = Path.GetFullPath(args[0]);
        var outputDirectory = Path.GetFullPath(args[1]);

        Console.WriteLine($"Source Directory: {sourceDirectory}");
        Console.WriteLine($"Output Directory: {outputDirectory}");
        Console.WriteLine();

        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine($"Error: Source directory does not exist: {sourceDirectory}");
            Environment.Exit(1);
        }

        try
        {
            var generator = new MarkdownToRazorGenerator();
            await generator.GenerateRazorPagesAsync(sourceDirectory, outputDirectory);

            Console.WriteLine();
            Console.WriteLine("✅ Code generation completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during code generation: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}
