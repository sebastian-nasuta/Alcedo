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
                ChatMessageContentPart.CreateTextPart("You firstly generate 10 tags that describe only the illustration, not the style or technique."),
                ChatMessageContentPart.CreateTextPart("In next step you should generate 5 tags to describe the style and finaly 5 tags to describe the technique."),
                ChatMessageContentPart.CreateTextPart("Every tag should start with a '#' symbol."),
                ChatMessageContentPart.CreateTextPart("Separate tags with spaces. Dont separate tags with commas or any other characters."),
                ChatMessageContentPart.CreateTextPart("Every image probably will be ink, pencil or coloured pencils drawing. If not you must return tags anyway, but 1st tag must be named: '#probablyNotDrawing' or '#definitelyNotDrawing' (You must evaluate which one is more appropriate)."),
                ChatMessageContentPart.CreateTextPart("Sometimes image will be part of word or sentence written on paper."),
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
