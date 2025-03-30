using System.Text.Json.Serialization;

namespace PlotPocket.Server.Models.Responses;

public class TrendingResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("results")]
    public List<Trending> Results { get; set; } = new();

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }
}

public class Trending : ApiMediaItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("media_type")]
    public string MediaType { get; set; }

    [JsonPropertyName("first_air_date")]
    public string FirstAirDate { get; set; }

    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; }


    public string DisplayTitle => !string.IsNullOrEmpty(Title) ? Title : Name;


    public DateTime? DisplayDate =>
        !string.IsNullOrEmpty(ReleaseDate) ? DateTime.Parse(ReleaseDate) :
        !string.IsNullOrEmpty(FirstAirDate) ? DateTime.Parse(FirstAirDate) :
        null;
}