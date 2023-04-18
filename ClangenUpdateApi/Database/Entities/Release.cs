namespace ClangenUpdateApi.Database.Entities;

public class Release
{
    public int ReleaseId { get; set; }
    public string VersionNumber { get; set; } = null!;
    public string BuildRef { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsVisible { get; set; } = true;
    public List<Artifact> Artifacts { get; set; } = new();
    public List<ReleaseChannel> ReleaseChannels { get; set; } = new();
}
