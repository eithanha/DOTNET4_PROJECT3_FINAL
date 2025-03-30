using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;


namespace PlotPocket.Server.Models;

public class ApplicationUser : IdentityUser
{
    public virtual ICollection<Show> Shows { get; set; } = new HashSet<Show>();
}