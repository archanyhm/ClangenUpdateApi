using ClangenUpdateApi.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClangenUpdateApi.Database;

public class MainContext : DbContext
{
    public DbSet<ReleaseChannel> ReleaseChannels { get; set; } = null!;
    public DbSet<Release> Releases { get; set; } = null!;
    public DbSet<Artifact> Artifacts { get; set; } = null!;

    private static string ConnectionString => Environment.GetEnvironmentVariable("DATABASE_URL") ??
                                               "Host=localhost;Database=clangen;Username=postgres;Password=postgres";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(ConnectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReleaseChannel>()
            .HasOne(releaseChannel => releaseChannel.LatestRelease)
            .WithOne()
            .HasForeignKey<ReleaseChannel>(releaseChannel => releaseChannel.LatestReleaseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        base.OnModelCreating(modelBuilder);
    }
}
