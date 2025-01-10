using Alcedo.Services.ImageTaggingService;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Alcedo.Services.ImageTaggingService;

internal abstract class ImageTaggingServiceBase : IImageTaggingService
{
    protected static Dictionary<string, string[]> GenerateInktoberTags(ref string? customTag)
    {
        var tagsDictionary = new Dictionary<string, string[]>();
        if (!string.IsNullOrWhiteSpace(customTag))
        {
            customTag = customTag.Trim().TrimStart('#');

            tagsDictionary.Add(
                "inktober",
                [
                    "#" + customTag,
                    "#inktober",
                    "#inktober" + customTag,
                    "#inktober" + DateTime.UtcNow.Year,
                    "#inktober" + DateTime.UtcNow.Year + customTag,
                    "#inktober52",
                    "#inktober52" + customTag,
                    "#ink",
                    "#drawing",
                ]);
        }

        return tagsDictionary;
    }

    protected async Task<string> TestConnectionAsync(string url, string? apiKey = null)
    {
        try
        {
            using var client = new HttpClient();
            if (!string.IsNullOrEmpty(apiKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            }
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            throw new Exception($"An error occurred while testing the connection: {ex.Message}");
        }
    }

    protected static string[] GetTagGenerationInstructionParts(Dictionary<string, string[]> tagsDictionary)
    {
        var result = new List<string>
        {
            """
            You are an assistant that generates tags for Instagram images sent to you in base64 format.
            """,
            """
            ~TASKS~
            1. Generate 10 tags that describe only the illustration, ignoring the style or technique.
            2. Generate 5 tags to describe the style.
            3. Generate 5 tags to describe the technique.
            """,
            """
            ~RULES~
            - return only tags grouped in arrays named "general", "style" and "technique" in json format
            - every tag should have prefix '#'
            - all tags should consist of letters and numbers, without special characters
            - if a tag consists of multiple words, use camelCase
            """
        };

        if (tagsDictionary.Count != 0)
        {
            var existingTags = string.Join(", ", tagsDictionary.SelectMany(x => x.Value));
            result.Add($"""
                ~EXISTING TAGS~
                The following tags are already present and you can never generate any of them again:
                {existingTags}
                """);
        }

        result.Add("""
            ~EXAMPLE~
            {
              "general": [
                "#sunset",
                "#ocean",
                "#waves",
                "#beach",
                "#palmTrees",
                "#silhouette",
                "#sky",
                "#clouds",
                "#scenic",
                "#horizon"
              ],
              "style": [
                "#realistic",
                "#vibrant",
                "#colorful",
                "#detailed",
                "#naturalistic"
              ],
              "technique": [
                "#oilPainting",
                "#brushStrokes",
                "#layering",
                "#blending",
                "#impasto"
              ]
            }
            """);

        return [.. result];
    }

    public abstract Task<string> GetTagDescriptionAsync(string tag);
    public abstract Task<ILookup<string, string>> GetTagsAsync(string base64Image, string? customTag = null);
    public abstract Task<string> TestConnectionAsync();
    public abstract Task<string> TestImageRecognitionAsync(string base64Image);
}