using Microsoft.FluentUI.AspNetCore.Components;
using MarkdownToRazor.Services;
using MarkdownToRazor.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add MDFileToRazor services for runtime markdown rendering
builder.Services.AddMarkdownToRazorServices(options =>
{
    options.SourceDirectory = "content"; // Where your markdown files are located
    options.BaseRoutePath = "/docs"; // Optional base route path
    // OutputDirectory is not needed for runtime-only scenarios
    // DefaultLayout is not needed - component will use app's default layout
});

// Alternative simple configurations:
// builder.Services.AddMarkdownToRazorServices(); // Use defaults
// builder.Services.AddMarkdownToRazorServices("content"); // Custom source only
// builder.Services.AddMarkdownToRazorServices("content", "Pages/Auto"); // Custom source & output

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
