using MDFIleTORazor.CodeGeneration;

// Test the route processing logic
var generator = new MarkdownToRazorGenerator();

// Create a simple test
Directory.CreateDirectory("input");
Directory.CreateDirectory("output");

await File.WriteAllTextAsync("input/test.md", @"---
route: /
title: Home Page
---

# Welcome");

await generator.GenerateRazorPagesAsync("input", "output");

var result = await File.ReadAllTextAsync("output/Test.razor");
Console.WriteLine("Generated file:");
Console.WriteLine(result);

// Cleanup
Directory.Delete("input", true);
Directory.Delete("output", true);
