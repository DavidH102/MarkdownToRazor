using Microsoft.FluentUI.AspNetCore.Components;
using MDFileToRazor.Components.Services;
using MDFileToRazor.Components.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add MDFileToRazor services for runtime markdown rendering
builder.Services.AddMdFileToRazorServices(options =>
{
    options.SourceDirectory = "content"; // Where your markdown files are located
    options.BaseRoutePath = "/docs"; // Optional base route path
    // OutputDirectory is not needed for runtime-only scenarios
    // DefaultLayout is not needed - component will use app's default layout
});

// Alternative simple configurations:
// builder.Services.AddMdFileToRazorServices(); // Use defaults
// builder.Services.AddMdFileToRazorServices("content"); // Custom source only
// builder.Services.AddMdFileToRazorServices("content", "Pages/Auto"); // Custom source & output

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
