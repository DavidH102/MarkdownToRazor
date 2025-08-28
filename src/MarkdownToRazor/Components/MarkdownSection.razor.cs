// ------------------------------------------------------------------------
// This file is licensed to you under the MIT License.
// ------------------------------------------------------------------------

using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using MarkdownToRazor.Services;

namespace MarkdownToRazor.Components;

public partial class MarkdownSection : FluentComponentBase
{
    private IJSObjectReference _jsModule = default!;
    private bool _markdownChanged = false;
    private string? _previousContent;
    private string? _previousFromAsset;
    private string? _previousFilePath;

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IStaticAssetService StaticAssetService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the Markdown content
    /// </summary>
    [Parameter]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets asset to read the Markdown from
    /// </summary>
    [Parameter]
    public string? FromAsset { get; set; }

    /// <summary>
    /// Gets or sets the file path to read the Markdown from (alias for FromAsset)
    /// </summary>
    [Parameter]
    public string? FilePath { get; set; }

    [Parameter]
    public EventCallback OnContentConverted { get; set; }

    public MarkupString HtmlContent { get; private set; }

    protected override void OnInitialized()
    {
        if (Content is null && string.IsNullOrEmpty(FromAsset) && string.IsNullOrEmpty(FilePath))
        {
            throw new ArgumentException("You need to provide either Content, FromAsset, or FilePath parameter");
        }
    }

    protected override void OnParametersSet()
    {
        // Check if any content-related parameters have changed
        if ((_previousContent != Content) ||
            (_previousFromAsset != FromAsset) ||
            (_previousFilePath != FilePath))
        {
            _markdownChanged = true;
            _previousContent = Content;
            _previousFromAsset = FromAsset;
            _previousFilePath = FilePath;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // import code for highlighting code blocks
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
                "./Components/MarkdownSection.razor.js");
        }

        if (firstRender || _markdownChanged)
        {
            _markdownChanged = false;

            // create markup from markdown source
            HtmlContent = await MarkdownToMarkupStringAsync();
            StateHasChanged();

            // notify that content converted from markdown
            if (OnContentConverted.HasDelegate)
            {
                await OnContentConverted.InvokeAsync();
            }
            await _jsModule.InvokeVoidAsync("highlight");
            await _jsModule.InvokeVoidAsync("addCopyButton");
        }
    }

    /// <summary>
    /// Converts markdown, provided in Content or from markdown file loaded via StaticAssetService, to MarkupString for rendering.
    /// </summary>
    /// <returns>MarkupString</returns>
    private async Task<MarkupString> MarkdownToMarkupStringAsync()
    {
        string? markdown;
        if (string.IsNullOrEmpty(FromAsset) && string.IsNullOrEmpty(FilePath))
        {
            Console.WriteLine("MarkdownSection: Using provided Content");
            markdown = Content;
        }
        else
        {
            // Use FilePath if provided, otherwise use FromAsset
            var fileToLoad = !string.IsNullOrEmpty(FilePath) ? FilePath : FromAsset;
            Console.WriteLine($"MarkdownSection: Loading file: {fileToLoad}");

            markdown = await StaticAssetService.GetAsync(fileToLoad!);

            if (string.IsNullOrEmpty(markdown))
            {
                Console.WriteLine($"MarkdownSection: File not found or empty: {fileToLoad}");
                markdown = $"File not found: {fileToLoad}";
            }
            else
            {
                Console.WriteLine($"MarkdownSection: Successfully loaded {markdown.Length} characters");
            }
        }

        return ConvertToMarkupString(markdown);
    }
    private static MarkupString ConvertToMarkupString(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var builder = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Use<MarkdownSectionPreCodeExtension>();

            var pipeline = builder.Build();

            // Convert markdown string to HTML
            var html = Markdown.ToHtml(value, pipeline);

            // Return sanitized HTML as a MarkupString that Blazor can render
            return new MarkupString(html);
        }

        return new MarkupString();
    }
}
