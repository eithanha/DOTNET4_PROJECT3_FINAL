using System;

namespace PlotPocket.Server.Models
{
    public class Bookmark
    {
        public int Id { get; set; }
        public int ShowId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}