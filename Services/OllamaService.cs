﻿using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alcedo.Services;

internal class OllamaService
{
    private static readonly string ollamaUrl = "http://192.168.0.81:11434";
    private static readonly string ollamaApiUrl = ollamaUrl + "/api";
    private static readonly string ollamaApiChatUrl = ollamaApiUrl + "/chat";
    private static readonly string ollamaApiGenerateUrl = ollamaApiUrl + "/generate";

    internal static async Task<string> TestOllamaConnection()
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

    internal static async Task<string> GetTagDescriptionAsync(string tag, Action<string>? onPartialResponse = null)
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
                stream = onPartialResponse is not null
            };

            var serializedRequestBody = JsonSerializer.Serialize(requestBody);

            using var client = new HttpClient();
            var response = await client.PostAsync(
                ollamaApiChatUrl,
                new StringContent(serializedRequestBody, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(responseStream);

            if (onPartialResponse is not null)
            {
                throw new NotImplementedException("Code requires testing and debugging before being used.");

                char[] buffer = new char[1024];
                int bytesRead;
                while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    var partialResponse = new string(buffer, 0, bytesRead);
                    onPartialResponse(partialResponse);
                }
            }

            var responseContent = await reader.ReadToEndAsync();

            var result = JsonSerializer.Deserialize<OllamaResponse>(responseContent);

            return result?.Message.Content ?? string.Empty;
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            throw new Exception($"An error occurred while generating tags: {ex.Message}");
        }
    }

    private class OllamaResponse
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