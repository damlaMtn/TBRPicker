namespace TBRPicker.DTOs;

public class AiRecommendationRequest
{
    public string Mood { get; set; } = string.Empty;
    public int? MaxPages { get; set; }
    public string? Shelf { get; set; }
    public string? Genre { get; set; }
}