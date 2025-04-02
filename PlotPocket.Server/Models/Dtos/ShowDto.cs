using System.Text.Json.Serialization;

namespace PlotPocket.Server.Models.Dtos;

public class ShowDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Overview { get; set; }
    public string PosterPath { get; set; }
    public double Rating { get; set; }
    public DateTime? ReleaseDate { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ShowType Type { get; set; }

    public bool IsWatchlisted { get; set; }
    public bool IsWatched { get; set; }
    public bool IsBookmarked { get; set; }
}

public enum ShowType
{
    Movie,
    TvShow
}
