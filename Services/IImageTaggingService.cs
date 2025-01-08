
namespace Alcedo.Services
{
    public interface IImageTaggingService
    {
        Task<string> TestConnectionAsync();
        Task<string> GetTagDescriptionAsync(string tag);
        Task<ILookup<string, string>> GetTagsAsync(string base64Image, string? customTag = null);
    }
}