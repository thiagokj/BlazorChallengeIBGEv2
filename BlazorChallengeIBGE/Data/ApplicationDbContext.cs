using BlazorChallengeIBGE.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorChallengeIBGE.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Locality>().ToTable("IbgeLocality");
    modelBuilder.Entity<State>().ToTable("IbgeState");

    modelBuilder.Entity<Locality>()
         .HasOne(l => l.State)
         .WithMany()
         .HasForeignKey(l => l.StateId)
         .IsRequired();

    modelBuilder.Entity<Locality>()
      .HasIndex(l => new { l.StateId, l.City })
      .IsUnique();
  }

  public DbSet<Locality> Localities { get; set; } = null!;
  public DbSet<State> States { get; set; } = null!;
}
