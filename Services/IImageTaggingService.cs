
namespace Alcedo.Services
{
    public interface IImageTaggingService
    {
        Task<string> TestConnectionAsync();
        Task<ILookup<string, string>> GetTagsAsync(string base64Image, string? customTag = null);
        Task<string> GetTagDescriptionAsync(string tag, Action<string>? onPartialResponse = null);
    }
}