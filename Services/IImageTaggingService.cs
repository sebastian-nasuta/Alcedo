
namespace Alcedo.Services
{
    public interface IImageTaggingService
    {
        Task<string> TestOllamaConnection();
        Task<ILookup<string, string>> GetTagsAsync(string base64Image, string? customTag = null);
        Task<string> GetTagDescriptionAsync(string tag, Action<string>? onPartialResponse = null);
    }
}