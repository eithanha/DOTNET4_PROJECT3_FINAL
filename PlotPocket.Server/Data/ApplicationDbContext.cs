using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlotPocket.Server.Models;

namespace PlotPocket.Server.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Show> Shows { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Show>()
            .HasMany(s => s.Users)
            .WithMany(u => u.Shows)
            .UsingEntity(j => j.ToTable("UserShows"));

        builder.Entity<Bookmark>()
            .HasKey(b => b.Id);

        builder.Entity<Bookmark>()
            .HasIndex(b => new { b.ShowId, b.UserId })
            .IsUnique();
    }
}
