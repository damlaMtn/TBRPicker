using HtmlAgilityPack;

namespace TBRPicker.Services;

public class GoodreadsScraperService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoodreadsScraperService> _logger;

    public GoodreadsScraperService(HttpClient httpClient, ILogger<GoodreadsScraperService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<string>> GetShelvesAsync(string userId)
    {
        var url = $"https://www.goodreads.com/review/list/{userId}";
        var shelves = new List<string>();

        try
        {
            var html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Shelf links sit in the left panel under #paginatedShelfList
            var shelfNodes = doc.DocumentNode
                .SelectNodes("//div[@id='paginatedShelfList']//a[@class='actionLinkLite']");

            if (shelfNodes is null)
            {
                _logger.LogWarning("No shelves found for user {UserId} — profile may be private or markup changed", userId);
                return shelves;
            }

            foreach (var node in shelfNodes)
            {
                var name = node.InnerText.Trim();
                // Each link text looks like "to-read (87)" — we just want "to-read"
                var cleanName = System.Text.RegularExpressions.Regex.Match(name, @"^[^\(]+").Value.Trim();
                if (!string.IsNullOrEmpty(cleanName))
                    shelves.Add(cleanName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch shelves for user {UserId}", userId);
        }

        return shelves;
    }
}