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
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("TEXT");
            entity.Property(e => e.Username).HasColumnType("TEXT").IsRequired();
            entity.Property(e => e.Password).HasColumnType("TEXT").IsRequired();
            entity.HasData(new User { Id = Guid.Parse("4c05e191-2c9e-4eb9-a7e1-881c15b14441"), Username = "admin", Password = "admin" });
        });

        modelBuilder.Entity<Story>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("TEXT");
            entity.Property(e => e.Title).HasColumnType("TEXT").IsRequired();
            entity.Property(e => e.Json).HasColumnType("TEXT").IsRequired();

            entity.HasData(new Story {
                Id = Guid.Parse("e819b5c2-f170-4eb6-9280-928570e30d12"),
                Title = "Test",
                Json = "{\"startNode\":\"start\",\"nodes\":{\"start\":{\"text\":\"New <b>Story</b>\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-7dwemcw\",\"text\":\"Go up\",\"next\":\"up\",\"media\":[]},{\"id\":\"c-ymjrgmy\",\"text\":\"Go down\",\"next\":\"down\",\"media\":[]},{\"id\":\"c-wfz80t0\",\"text\":\"Go left\",\"next\":\"Left\",\"media\":[]},{\"id\":\"c-hatj1zf\",\"text\":\"Go right\",\"next\":\"right\",\"media\":[]}]},\"Left\":{\"text\":\"Left room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-a5li9pt\",\"text\":\"UP\",\"next\":\"UL-END\",\"media\":[]},{\"id\":\"c-eomy6ee\",\"text\":\"DOWN\",\"next\":\"DL\",\"media\":[]},{\"id\":\"c-sssocou\",\"text\":\"Right\",\"next\":\"start\",\"media\":[]}]},\"right\":{\"text\":\"Right room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-ptauk0f\",\"text\":\"<span style=\\\"color: red\\\">UP</span>\",\"next\":\"UR\",\"media\":[]},{\"id\":\"c-ch8c9ts\",\"text\":\"DOWN\",\"next\":\"DR\",\"media\":[]},{\"id\":\"c-ef9mtv7\",\"text\":\"LEFT\",\"next\":\"start\",\"media\":[]}]},\"up\":{\"text\":\"Up room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-h5vyuxr\",\"text\":\"Left\",\"next\":\"UL-END\",\"media\":[]},{\"id\":\"c-4aqymv3\",\"text\":\"Right\",\"next\":\"UR\",\"media\":[]},{\"id\":\"c-a5xl0pl\",\"text\":\"Down\",\"next\":\"start\",\"media\":[]}]},\"down\":{\"text\":\"Down room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-2gjqpn1\",\"text\":\"Left\",\"next\":\"DL\",\"media\":[]},{\"id\":\"c-mj6bp7g\",\"text\":\"Right\",\"next\":\"DR\",\"media\":[]},{\"id\":\"c-vovbuug\",\"text\":\"Up\",\"next\":\"start\",\"media\":[]}]},\"UL-END\":{\"text\":\"Up Left Room - The END\",\"isEnding\":true,\"media\":[],\"choices\":[]},\"DL\":{\"text\":\"Down Left Room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-wi1jeq3\",\"text\":\"Up\",\"next\":\"Left\",\"media\":[]},{\"id\":\"c-f9ww0vy\",\"text\":\"Right\",\"next\":\"down\",\"media\":[]}]},\"UR\":{\"text\":\"Up Right Room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-wo0sjkg\",\"text\":\"Left\",\"next\":\"up\",\"media\":[]},{\"id\":\"c-hcti6wp\",\"text\":\"Down\",\"next\":\"right\",\"media\":[]}]},\"DR\":{\"text\":\"Down Right Room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-nxncv8s\",\"text\":\"Up\",\"next\":\"right\",\"media\":[]},{\"id\":\"c-kdqenhp\",\"text\":\"Left\",\"next\":\"down\",\"media\":[]}]}}}",
                CreatedAt = System.DateTime.Parse("2026-09-09 11:36:30"),
                UpdatedAt = System.DateTime.Parse("2026-09-10 07:48:46"),
                MaxPlaythroughs = 2,
                Password = "pass",
                Description = "Test <b>story</b> \n"
            });
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
