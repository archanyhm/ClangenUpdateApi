using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ClangenUpdateApi.Database.Entities;

[Index(nameof(Name), IsUnique = true)]
public class ReleaseChannel
{
    public int ReleaseChannelId { get; set; }
    [Required] public string Name { get; set; }
    public List<Release> Releases { get; set; } = new();
    public int? LatestReleaseId { get; set; }
    public Release? LatestRelease { get; set; }
}
