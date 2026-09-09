using InteractiveStory.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Story> Stories { get; set; } = null!;
    public DbSet<Playthrough> Playthroughs { get; set; } = null!;
    public DbSet<PlaythroughHistory> PlaythroughHistories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Story>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("TEXT");
            entity.Property(e => e.Title).HasColumnType("TEXT").IsRequired();
            entity.Property(e => e.Json).HasColumnType("TEXT").IsRequired();
        });

        modelBuilder.Entity<Playthrough>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Story)
                  .WithMany()
                  .HasForeignKey(e => e.StoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.CurrentNodeId).HasColumnType("TEXT").IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<PlaythroughHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Playthrough)
                  .WithMany() // Assuming we don't have a navigation collection on Playthrough for simplicity
                  .HasForeignKey(e => e.PlaythroughId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.NodeId).HasColumnType("TEXT").IsRequired();
            entity.Property(e => e.ChosenChoiceId).HasColumnType("TEXT");
        });
    }
}
