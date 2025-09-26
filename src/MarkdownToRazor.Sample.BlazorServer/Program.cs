using MarkdownToRazor.Sample.BlazorServer.Components;
using MarkdownToRazor.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MarkdownToRazor services for build-time generation demo
builder.Services.AddMarkdownToRazorServices(options =>
{
    options.SourceDirectory = "MDFilesToConvert";
    options.OutputDirectory = "Pages/Generated";
    options.SearchRecursively = true;
    options.EnableYamlFrontmatter = true;
    options.EnableHtmlCommentConfiguration = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
