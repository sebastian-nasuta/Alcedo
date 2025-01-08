
namespace Alcedo.Services.ImageTaggingService
{
    public interface IImageTaggingService
    {
        Task<string> TestConnectionAsync();
        Task<string> GetTagDescriptionAsync(string tag);
        Task<string> TestImageRecognitionAsync(string base64Image);
        Task<ILookup<string, string>> GetTagsAsync(string base64Image, string? customTag = null);
    }
}