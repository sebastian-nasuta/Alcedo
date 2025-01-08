using System.Text.Json.Serialization;

namespace Alcedo.Services.ImageTaggingService;

internal partial class OllamaImageTaggingService
{
    public class OllamaResponse
    {
        [JsonPropertyName("message")]
        public MessageModel Message { get; set; } = null!;

        public class MessageModel
        {
            [JsonPropertyName("content")]
            public string Content { get; set; } = null!;
        }
    }
}
