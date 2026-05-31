using System.Text;
using System.Text.Json;
using TBRPicker.Models;

namespace TBRPicker.Services;

public class AiRecommendationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiRecommendationService> _logger;

    public AiRecommendationService(HttpClient httpClient, IConfiguration configuration, ILogger<AiRecommendationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AiRecommendationResult?> RecommendAsync(string mood, List<Book> books)
    {
        var apiKey = _configuration["Anthropic:ApiKey"];

        var bookList = books.Select((b, i) =>
            $"{i + 1}. \"{b.Title}\" by {b.Author}" +
            (b.Genre != null ? $" [{b.Genre}]" : "") +
            (b.PageCount.HasValue ? $" ({b.PageCount} pages)" : "")
        );

        var prompt = $"You are a thoughtful book recommender. A reader describes their mood as: \"{mood}\"\n\n" +
                     $"From the following list, pick the single best matching book and explain why in 1-2 warm, friendly sentences.\n\n" +
                     $"Books:\n{string.Join("\n", bookList)}\n\n" +
                     "Respond ONLY with a valid JSON object in this exact format, no other text:\n" +
                     "{\n" +
                     "    \"title\": \"exact title from the list\",\n" +
                     "    \"author\": \"exact author from the list\",\n" +
                     "    \"reason\": \"your warm explanation here\"\n" +
                     "}";

        var requestBody = new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 300,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Anthropic API error: {Body}", responseBody);
            return null;
        }

        var parsed = JsonDocument.Parse(responseBody);
        var text = parsed.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrEmpty(text)) return null;

        // Strip markdown code fences if present
        text = text.Trim();
        if (text.StartsWith("```"))
        {
            text = string.Join("\n", text.Split('\n').Skip(1));
            text = text.Substring(0, text.LastIndexOf("```")).Trim();
        }

        var result = JsonSerializer.Deserialize<AiRecommendationResult>(text, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }
}