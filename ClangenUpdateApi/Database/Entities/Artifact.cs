using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClangenUpdateApi.Database.Entities;

[Index(nameof(Name), nameof(ReleaseId), IsUnique = true)]
public class Artifact
{
   public int ArtifactId { get; set; } 
   public int ReleaseId { get; set; }
   public Release Release { get; set; } = null!;
   public string Name { get; set; } = null!;
   public string FileName { get; set; } = null!;
   [Column(TypeName = "oid")] public uint FileOid { get; set; }
   public string? GpgSignature { get; set; }
   public long FileSize { get; set; }
   public ulong DownloadCount { get; set; }
   public bool HasValidSignature { get; set; }
}
