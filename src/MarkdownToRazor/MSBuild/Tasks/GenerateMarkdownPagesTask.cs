using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MarkdownToRazor.MSBuild.Tasks;

/// <summary>
/// MSBuild task to generate Razor pages from Markdown files
/// </summary>
public class GenerateMarkdownPagesTask : Microsoft.Build.Utilities.Task
{
    /// <summary>
    /// Source directory containing Markdown files
    /// </summary>
    [Required]
    public string? SourceDirectory { get; set; }

    /// <summary>
    /// Output directory for generated Razor pages
    /// </summary>
    [Required]
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Execute the task
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public override bool Execute()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SourceDirectory))
            {
                Log.LogError("SourceDirectory is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(OutputDirectory))
            {
                Log.LogError("OutputDirectory is required");
                return false;
            }

            if (!Directory.Exists(SourceDirectory))
            {
                Log.LogWarning($"Source directory does not exist: {SourceDirectory}");
                return true; // Not an error if source doesn't exist
            }

            Log.LogMessage(MessageImportance.Normal,
                $"Generating Razor pages from Markdown files in: {SourceDirectory}");
            Log.LogMessage(MessageImportance.Normal,
                $"Output directory: {OutputDirectory}");

            // Use the MarkdownToRazorGenerator to generate pages
            var generator = new MarkdownToRazor.CodeGeneration.MarkdownToRazorGenerator();
            generator.GenerateRazorPagesAsync(SourceDirectory, OutputDirectory).Wait();

            Log.LogMessage(MessageImportance.Normal,
                "Successfully generated Razor pages from Markdown files");

            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Error generating Razor pages: {ex.Message}");
            Log.LogMessage(MessageImportance.Low, $"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}
