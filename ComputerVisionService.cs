using OpenAI.Chat;

namespace Alcedo;

internal class ComputerVisionService
{
    private static readonly string apiKey = "";

    /// <summary>
    /// This method generates 10 tags that describe the image from the base64Image parameter.
    /// </summary>
    internal static async Task<string[]> GetTagsAsync(string base64Image)
    {
        try
        {
            var model = "gpt-3.5-turbo";
            model = "gpt-4o";
            ChatClient client = new(model: model, apiKey: apiKey);

            ChatMessage chatMessage = new UserChatMessage([
                ChatMessageContentPart.CreateTextPart("You are an assistant that generates tags for instagram image send to you in base64 format."),
                ChatMessageContentPart.CreateTextPart("Every image is ink or pencil drawing."),
                ChatMessageContentPart.CreateTextPart("You always generate 10 tags for each image."),
                ChatMessageContentPart.CreateTextPart("Every tag should start with a '#' symbol."),
                ChatMessageContentPart.CreateTextPart("Separate tags with spaces. Dont separate tags with commas or any other characters."),
                ChatMessageContentPart.CreateImagePart(new Uri($"data:image/png;base64,{base64Image}"))
            ]);

            ChatCompletion completion = await client.CompleteChatAsync(chatMessage);

            return [completion.Content[0].Text];
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            throw new Exception($"An error occurred while generating tags: {ex.Message}");
        }
    }
}
