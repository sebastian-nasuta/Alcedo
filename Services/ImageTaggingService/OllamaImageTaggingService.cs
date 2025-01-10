using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Alcedo.Services.ImageTaggingService;

internal partial class OllamaImageTaggingService(IConfiguration configuration) : ImageTaggingServiceBase
{
    private string? _ollamaUrl;
    private string OllamaUrl => _ollamaUrl ??= configuration["Ollama:Url"]
        ?? throw new ArgumentNullException("Ollama:Url is missing in the configuration file.");

    private string OllamaApiUrl => OllamaUrl + "/api";
    private string OllamaApiChatUrl => OllamaApiUrl + "/chat";

    public override async Task<string> TestConnectionAsync()
        => await TestConnectionAsync(OllamaUrl);

    public override async Task<string> GetTagDescriptionAsync(string tag)
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
                OllamaApiChatUrl,
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

    public override async Task<string> TestImageRecognitionAsync(string base64Image)
    {
        try
        {
            var userMessage = new
            {
                role = "user",
                content = "what is in this image?",
                images = new[] { base64Image }
            };

            var requestBody = new
            {
                model = "llama3.2-vision",
                messages = new[] { userMessage }
            };

            var serializedRequestBody = JsonSerializer.Serialize(requestBody);

            using var client = new HttpClient();
            var response = await client.PostAsync(
                OllamaApiChatUrl,
                new StringContent(serializedRequestBody, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(responseStream);

            var responseContent = await reader.ReadToEndAsync();

            var result = JsonSerializer.Deserialize<OllamaResponse>(responseContent)
                ?? throw new Exception("An error occurred while deserializing the tags. responseContent: " + responseContent);

            return result.Message.Content;
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
                OllamaApiChatUrl,
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
        var instructionParts = GetTagGenerationInstructionParts(tagsDictionary);
        foreach (var instructionPart in instructionParts)
        {
            sb.AppendLine(instructionPart);
        }

        return new
        {
            role = "system",
            content = sb.ToString()
        };
    }
}