# Temporary script to test the new docs/ routing
Write-Host "Testing updated code generation with docs/ routing..."

# Create a temporary console app for testing
$tempProject = "TempGenerator"
Remove-Item -Path $tempProject -Recurse -Force -ErrorAction SilentlyContinue
dotnet new console -n $tempProject
cd $tempProject

# Add reference to our code generation library
dotnet add reference ../src/MDFileToRazor.CodeGeneration/MDFileToRazor.CodeGeneration.csproj

# Replace Program.cs with our generator code
@'
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
'@ | Out-File -Path "Program.cs" -Encoding UTF8

# Build and run the test
dotnet build
if ($LASTEXITCODE -eq 0) {
    Write-Host "Running code generation test..."
    dotnet run -- "../samples/MDFilesToConvert" "../samples/Pages/Generated"
}

cd ..
