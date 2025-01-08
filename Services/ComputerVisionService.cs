using OpenAI.Chat;
using System.Text.Json;

namespace Alcedo.Services;

internal class ComputerVisionService
{
    private static readonly string apiKey = "";

    /// <summary>
    /// This method generates tags that describe the image from the base64Image parameter.
    /// </summary>
    internal static async Task<ILookup<string, string>> GetTagsAsync(string base64Image, string? customTag = null)
    {
        try
        {
            var tagsDictionary = new Dictionary<string, string[]>();
            if (!string.IsNullOrWhiteSpace(customTag))
            {
                customTag = customTag.Trim();

                tagsDictionary.Add(
                    "inktober",
                    [
                        customTag,
                        "inktober",
                        "inktober" + customTag,
                        "inktober" + DateTime.UtcNow.Year,
                        "inktober" + DateTime.UtcNow.Year + customTag,
                        "inktober52",
                        "inktober52" + customTag,
                        "ink",
                        "drawing",
                        "blackAndWhite"
                    ]);
            }

            var model = "gpt-3.5-turbo";
            model = "gpt-4o";
            var client = new ChatClient(model: model, apiKey: apiKey);

            var systemMessage = BuildSystemMessage(tagsDictionary);
            var userMessage = BuildUserMessage(base64Image);

            var completion = await client.CompleteChatAsync(
                [systemMessage, userMessage],
                new ChatCompletionOptions()
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                });
            
            var tags = completion.Value.Content[0].Text;

            var deserializedTags = JsonSerializer.Deserialize<Dictionary<string, string[]>>(tags)
                ?? throw new Exception("An error occurred while deserializing the tags. Prompt response: " + tags);

            foreach (var kvp in deserializedTags)
            {
                tagsDictionary.Add(kvp.Key, kvp.Value);
            }

            return tagsDictionary
                .SelectMany(kvp => kvp.Value, (kvp, tag) => new { kvp.Key, Tag = tag })
                .ToLookup(x => x.Key, x => x.Tag);
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            throw new Exception($"An error occurred while generating tags: {ex.Message}");
        }
    }

    private static SystemChatMessage BuildSystemMessage(Dictionary<string, string[]> tagsDictionary)
    {
        List<ChatMessageContentPart> chatMessageParts =
        [
            ChatMessageContentPart.CreateTextPart("You are an assistant that generates tags for Instagram images sent to you in base64 format."),
            ChatMessageContentPart.CreateTextPart("""
                ~TASKS~
                1. Generate 10 tags that describe only the illustration, ignoring the style or technique.
                2. Generate 5 tags to describe the style.
                3. Generate 5 tags to describe the technique.
                """),
            ChatMessageContentPart.CreateTextPart("""
                ~RULES~
                - return only tags grouped in arrays named "general", "style" and "technique" in json format
                - don't add any prefix signs like '#'
                - all tags should consist of letters and numbers, without special characters
                - if a tag consists of multiple words, use camelCase
                """)
        ];

        if (tagsDictionary.Count != 0)
        {
            var existingTags = JsonSerializer.Serialize(tagsDictionary);
            chatMessageParts.Add(ChatMessageContentPart.CreateTextPart($"""
                ~EXISTING TAGS~
                The following tags are already present and should not be generated again:
                {existingTags}
                """));
        }

        chatMessageParts.Add(ChatMessageContentPart.CreateTextPart("""
                ~EXAMPLE~
                {
                  "general": [
                    "sunset",
                    "ocean",
                    "waves",
                    "beach",
                    "palmTrees",
                    "silhouette",
                    "sky",
                    "clouds",
                    "scenic",
                    "horizon"
                  ],
                  "style": [
                    "realistic",
                    "vibrant",
                    "colorful",
                    "detailed",
                    "naturalistic"
                  ],
                  "technique": [
                    "oilPainting",
                    "brushStrokes",
                    "layering",
                    "blending",
                    "impasto"
                  ]
                }
            """));

        return new SystemChatMessage(chatMessageParts);
    }

    private static UserChatMessage BuildUserMessage(string base64Image)
        => new(ChatMessageContentPart.CreateImagePart(new($"data:image/png;base64,{base64Image}")));
}
