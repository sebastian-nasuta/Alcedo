using System.Text;
using System.Text.Json;

namespace Alcedo.Services;

internal class OllamaService
{
    private static readonly string apiUrl = "http://192.168.0.81:11434/api/chat";

    internal static async Task<string> TestOllamaConnection()
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync("http://192.168.0.81:11434");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            throw new Exception($"An error occurred while testing the Ollama connection: {ex.Message}");
        }
    }

    internal static async Task<string> GetTagDescriptionAsync(string tag, Action<string>? onPartialResponse = null)
    {
        try
        {
            var systemMessage = BuildSystemMessage();
            var userMessage = BuildUserMessage(tag);

            var requestBody = new
            {
                model = "llama3.2",
                messages = new[] { systemMessage, userMessage },
                stream = onPartialResponse is not null
            };

            var serializedRequestBody = JsonSerializer.Serialize(requestBody);

            using var client = new HttpClient();
            var response = await client.PostAsync(
                apiUrl,
                new StringContent(serializedRequestBody, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(responseStream);

            if (onPartialResponse is not null)
            {
                char[] buffer = new char[1024];
                int bytesRead;
                while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    var partialResponse = new string(buffer, 0, bytesRead);
                    onPartialResponse(partialResponse);
                }
            }

            var responseContent = await reader.ReadToEndAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            throw new Exception($"An error occurred while generating tags: {ex.Message}");
        }
    }

    private static object BuildSystemMessage()
        => new { role = "system", content = """
            You are an assistant that generates description for tag given by user.
            
            ~TASKS~
            - Generate 3 sentences that describe this tag.
            
            ~RULES~
            - Return only this 3 sentences in numbered list.
            """ };

    private static object BuildUserMessage(string tag)
        => new { role = "user", content = tag };
}