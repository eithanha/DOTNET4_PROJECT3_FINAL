using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PlotPocket.Server.Models
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Show> Shows { get; set; } = new List<Show>();
    }
}