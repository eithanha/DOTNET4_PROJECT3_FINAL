using System.ComponentModel.DataAnnotations;

namespace PlotPocket.Server.Models;

public class EmailLoginDetails
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }
}