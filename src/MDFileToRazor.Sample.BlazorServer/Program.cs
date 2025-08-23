using Microsoft.FluentUI.AspNetCore.Components;
using MDFileToRazor.Components.Services;
using MDFileToRazor.Components.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add MDFileToRazor services with custom configuration
builder.Services.AddMdFileToRazorServices(options =>
{
    options.SourceDirectory = "content"; // Custom source directory
    options.OutputDirectory = "Pages/Generated"; // Custom output directory
    options.BaseRoutePath = "/docs"; // Optional base route path
    options.DefaultLayout = "MainLayout"; // Default layout for generated pages
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
