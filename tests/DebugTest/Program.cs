using System;
using System.IO;
using System.Threading.Tasks;
using MarkdownToRazor.CodeGeneration;

var tempDir = Path.GetTempPath();
var sourceDir = Path.Combine(tempDir, "test_source");
var outputDir = Path.Combine(tempDir, "test_output");

Directory.CreateDirectory(sourceDir);
Directory.CreateDirectory(outputDir);

// Create file with spaces test
var testFile = Path.Combine(sourceDir, "file with spaces.md");
await File.WriteAllTextAsync(testFile, @"---
title: File With Spaces
route: /spaced-file
---

# File with spaces in name");

var generator = new MarkdownToRazorGenerator();
await generator.GenerateRazorPagesAsync(sourceDir, outputDir);

// Check what files were actually created
Console.WriteLine("Files in output directory:");
var files = Directory.GetFiles(outputDir, "*.razor", SearchOption.AllDirectories);
foreach (var file in files)
{
    Console.WriteLine($"  {file}");
    var content = await File.ReadAllTextAsync(file);
    Console.WriteLine("Generated content:");
    Console.WriteLine(content);
}

// Cleanup
Directory.Delete(sourceDir, true);
Directory.Delete(outputDir, true);
