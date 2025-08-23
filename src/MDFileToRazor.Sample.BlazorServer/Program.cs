using Microsoft.FluentUI.AspNetCore.Components;
using MDFileToRazor.Components.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add FluentUI services
builder.Services.AddFluentUIComponents();

// Add HttpClient for StaticAssetService
builder.Services.AddHttpClient();

// Register StaticAssetService
builder.Services.AddScoped<IStaticAssetService, StaticAssetService>();

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
