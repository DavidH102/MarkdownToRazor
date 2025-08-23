namespace MDFileToRazor.Components.Services;

public interface IStaticAssetService
{
    Task<string?> GetAsync(string path);
}
