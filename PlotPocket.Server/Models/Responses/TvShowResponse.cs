using System.Text.Json.Serialization;

namespace PlotPocket.Server.Models.Responses;

public class TvShowResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("results")]
    public List<TvShow> Results { get; set; } = new();

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }
}

public class TvShow : ApiMediaItem
{
    [JsonPropertyName("backdrop_path")]
    public string BackdropPath { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("original_name")]
    public string OriginalName { get; set; }

    [JsonPropertyName("first_air_date")]
    public string FirstAirDate { get; set; }

    [JsonPropertyName("origin_country")]
    public List<string> OriginCountry { get; set; } = new();

    [JsonPropertyName("original_language")]
    public new string OriginalLanguage { get; set; }

    public DateTime? DisplayDate =>
        !string.IsNullOrEmpty(FirstAirDate) ? DateTime.Parse(FirstAirDate) : null;
}