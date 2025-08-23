using MDFIleTORazor.CodeGeneration;

if (args.Length != 2)
{
    Console.WriteLine("Usage: dotnet run <source-directory> <output-directory>");
    return 1;
}

var generator = new MarkdownToRazorGenerator();
await generator.GenerateRazorPagesAsync(args[0], args[1]);
Console.WriteLine("Generation completed!");
return 0;
