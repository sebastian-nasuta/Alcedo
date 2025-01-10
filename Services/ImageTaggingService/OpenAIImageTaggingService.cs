using Alcedo.Services.SettingsService;
using OpenAI.Chat;
using System.Text.Json;

namespace Alcedo.Services.ImageTaggingService;

internal class OpenAIImageTaggingService(ISettingsService settingsService) : ImageTaggingServiceBase
{
    private string ApiKey => settingsService.LoadApiKey();

    public override async Task<string> TestConnectionAsync()
        => await TestConnectionAsync("https://api.openai.com/v1/models", ApiKey);

    public override Task<string> GetTagDescriptionAsync(string tag)
        => throw new NotImplementedException();

    public override async Task<string> TestImageRecognitionAsync(string base64Image)
    {
        try
        {
            var client = new ChatClient(GPTModels.gpt_4o, ApiKey);

            var userMessage = new UserChatMessage([
                ChatMessageContentPart.CreateTextPart("What is in this image?"),
                ChatMessageContentPart.CreateImagePart(new($"data:image/png;base64,{base64Image}"))
            ]);

            var completion = await client.CompleteChatAsync(
                [userMessage],
                new ChatCompletionOptions()
                {
                    ResponseFormat = ChatResponseFormat.CreateTextFormat()
                });

            return completion.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            throw new Exception($"An error occurred while testing image recognition: {ex.Message}");
        }
    }

    public override async Task<ILookup<string, string>> GetTagsAsync(string base64Image, string? customTag = null)
    {
        try
        {
            var tagsDictionary = GenerateInktoberTags(ref customTag);
            var systemMessage = BuildSystemMessage(tagsDictionary);
            var userMessage = new UserChatMessage(ChatMessageContentPart.CreateImagePart(new($"data:image/png;base64,{base64Image}")));

            var client = new ChatClient(GPTModels.gpt_4o, ApiKey);
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
        List<ChatMessageContentPart> chatMessageParts = [];
        var instructionParts = GetTagGenerationInstructionParts(tagsDictionary);
        foreach (var instructionPart in instructionParts)
        {
            chatMessageParts.Add(ChatMessageContentPart.CreateTextPart(instructionPart));
        }

        return new SystemChatMessage(chatMessageParts);
    }

    private class GPTModels
    {
        public const string gpt_3_5_turbo = "gpt-3.5-turbo";
        public const string gpt_4o = "gpt-4o";
    }
}