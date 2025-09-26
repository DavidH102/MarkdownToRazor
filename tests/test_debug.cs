using System;
using System.IO;
using System.Threading.Tasks;
using MarkdownToRazor.CodeGeneration;

class Program
{
    static async Task Main(string[] args)
    {
        var tempDir = Path.GetTempPath();
        var sourceDir = Path.Combine(tempDir, "test_source");
        var outputDir = Path.Combine(tempDir, "test_output");

        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(outputDir);

        // Create test markdown file
        var testFile = Path.Combine(sourceDir, "example.md");
        await File.WriteAllTextAsync(testFile, @"---
route: /example
title: Example Page
layout: MainLayout
showTitle: true
description: Example page for testing
tags: [example, test]
---

# Example Content

This is test content.");

        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(sourceDir, outputDir);

        var generatedFile = Path.Combine(outputDir, "Example.razor");
        if (File.Exists(generatedFile))
        {
            var content = await File.ReadAllTextAsync(generatedFile);
            Console.WriteLine("Generated content:");
            Console.WriteLine(content);
        }
        else
        {
            Console.WriteLine("File not generated");
        }

        // Cleanup
        Directory.Delete(sourceDir, true);
        Directory.Delete(outputDir, true);
    }
}