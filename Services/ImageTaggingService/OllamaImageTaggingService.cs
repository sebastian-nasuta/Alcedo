using System.Text;
using System.Text.Json;

namespace Alcedo.Services.ImageTaggingService;

internal partial class OllamaImageTaggingService : IImageTaggingService
{
    private static readonly string ollamaUrl = "http://192.168.0.81:11434";
    private static readonly string ollamaApiUrl = ollamaUrl + "/api";
    private static readonly string ollamaApiChatUrl = ollamaApiUrl + "/chat";

    public async Task<string> TestConnectionAsync()
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(ollamaUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            throw new Exception($"An error occurred while testing the Ollama connection: {ex.Message}");
        }
    }

    public async Task<string> GetTagDescriptionAsync(string tag)
    {
        try
        {
            var systemMessage = new
            {
                role = "system",
                content = """
                    You are an assistant that generates description for tag given by user.
            
                    ~TASKS~
                    - Generate 3 sentences that describe this tag.
            
                    ~RULES~
                    - Return only this 3 sentences in numbered list.
                    """
            };

            var userMessage = new
            {
                role = "user",
                content = tag
            };

            var requestBody = new
            {
                model = "llama3.2",
                messages = new[] { systemMessage, userMessage },
                stream = false
            };

            var serializedRequestBody = JsonSerializer.Serialize(requestBody);

            using var client = new HttpClient();
            var response = await client.PostAsync(
                ollamaApiChatUrl,
                new StringContent(serializedRequestBody, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(responseStream);

            var responseContent = await reader.ReadToEndAsync();

            var result = JsonSerializer.Deserialize<OllamaResponse>(responseContent)
                ?? throw new Exception("An error occurred while deserializing the tag description. Prompt response: " + responseContent);

            return result.Message.Content;
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            throw new Exception($"An error occurred while generating tag description: {ex.Message}");
        }
    }

    public async Task<ILookup<string, string>> GetTagsAsync(string base64Image, string? customTag = null)
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

            var systemMessage = BuildSystemMessage(tagsDictionary);

            var userMessage = new
            {
                role = "user",
                images = new[] { base64Image }
            };

            var requestBody = new
            {
                model = "llama3.2-vision",
                messages = new[] { systemMessage, userMessage }
            };

            var serializedRequestBody = JsonSerializer.Serialize(requestBody);

            using var client = new HttpClient();
            var response = await client.PostAsync(
                ollamaApiChatUrl,
                new StringContent(serializedRequestBody, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(responseStream);

            var responseContent = await reader.ReadToEndAsync();

            var result = JsonSerializer.Deserialize<OllamaResponse>(responseContent)
                ?? throw new Exception("An error occurred while deserializing the tags. responseContent: " + responseContent);

            var deserializedTags = JsonSerializer.Deserialize<Dictionary<string, string[]>>(result.Message.Content)
                ?? throw new Exception("An error occurred while deserializing the tags. result.Message.Content: " + result.Message.Content);

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

    private static object BuildSystemMessage(Dictionary<string, string[]> tagsDictionary)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an assistant that generates tags for Instagram images sent to you in base64 format.");
        sb.AppendLine("""
            ~TASKS~
            1. Generate 10 tags that describe only the illustration, ignoring the style or technique.
            2. Generate 5 tags to describe the style.
            3. Generate 5 tags to describe the technique.
            """);
        sb.AppendLine("""
            ~RULES~
            - return only tags grouped in arrays named "general", "style" and "technique" in json format
            - don't add any prefix signs like '#'
            - all tags should consist of letters and numbers, without special characters
            - if a tag consists of multiple words, use camelCase
            """);

        if (tagsDictionary.Count != 0)
        {
            var existingTags = JsonSerializer.Serialize(tagsDictionary);
            sb.AppendLine($"""
                ~EXISTING TAGS~
                The following tags are already present and should not be generated again:
                {existingTags}
                """);
        }

        sb.AppendLine("""
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
            """);

        return new
        {
            role = "system",
            content = sb.ToString()
        };
    }
}