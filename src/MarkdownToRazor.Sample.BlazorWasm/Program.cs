using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MarkdownToRazor.Extensions;
using MarkdownToRazor.Sample.BlazorWasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add MarkdownToRazor services for WASM
builder.Services.AddMarkdownToRazorServices(options =>
{
    options.SourceDirectory = "wwwroot/content";
    options.SearchRecursively = true;
    options.EnableYamlFrontmatter = true;
    options.EnableHtmlCommentConfiguration = true;
});

await builder.Build().RunAsync();
