using System.ComponentModel.DataAnnotations;
using PlotPocket.Server.Models.Dtos;

namespace PlotPocket.Server.Models;

public class Show
{
    [Key]
    public int Id { get; set; }
    public int ShowApiId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string? PosterPath { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public ShowType Type { get; set; }
    public bool Watched { get; set; }
    public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
}