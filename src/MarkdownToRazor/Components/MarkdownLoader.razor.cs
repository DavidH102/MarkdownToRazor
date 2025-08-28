using Microsoft.AspNetCore.Components;
using MarkdownToRazor.Services;

namespace MarkdownToRazor.Components;

public partial class MarkdownLoader : ComponentBase
{
    [Inject]
    private IStaticAssetService StaticAssetService { get; set; } = default!;

    [Parameter, EditorRequired]
    public string FilePath { get; set; } = string.Empty;

    public bool IsLoading { get; private set; } = true;
    public bool HasError { get; private set; }
    public string? Content { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadContent();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (IsLoading || HasError || string.IsNullOrEmpty(Content))
        {
            await LoadContent();
        }
    }

    private async Task LoadContent()
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            HasError = true;
            IsLoading = false;
            return;
        }

        try
        {
            IsLoading = true;
            HasError = false;
            StateHasChanged();

            Content = await StaticAssetService.GetAsync(FilePath);

            if (string.IsNullOrEmpty(Content))
            {
                HasError = true;
            }
        }
        catch
        {
            HasError = true;
            Content = null;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    public async Task RetryLoad()
    {
        await LoadContent();
    }
}
